using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Plugin.CustomCamera.Abstractions;
using Android.Hardware;
using Java.IO;
using Android.Media;
//using System.IO;
//using Android.Graphics;

namespace Plugin.CustomCamera
{
    [Register("plugin.customcamera.android.CustomCameraView")]
    public class CustomCameraView :
        FrameLayout,
        ICustomCameraView,
        TextureView.ISurfaceTextureListener,
        Camera.IPictureCallback,
        Camera.IPreviewCallback,
        Camera.IShutterCallback
    //ISurfaceHolderCallback
    {
        Camera _camera;
        Activity _activity;
        Android.Graphics.SurfaceTexture _surface;
        Camera.CameraInfo _cameraInfo;
        int _w;
        int _h;
        string pictureName = "picture.jpg";
        bool _isCameraStarted = false;
        //int _correctedRotation;
        int _imageRotation;
        int _cameraHardwareRotation;

        public CustomCameraView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            this._activity = (Activity)context;

            if (CustomCameraInstance.CustomCameraView != null)
            {
                // set properties, otherwise they will be cleared
                _selectedCamera = CustomCameraInstance.CustomCameraView.SelectedCamera;
                _cameraOrientation = CustomCameraInstance.CustomCameraView.CameraOrientation;
            }

            var _textureView = new TextureView(context);
            _textureView.SurfaceTextureListener = this;
            AddView(_textureView);

            // make this view available in the PCL
            CustomCameraInstance.CustomCameraView = this;
        }

        #region ICustomCameraView interface implementation

        CameraOrientation _cameraOrientation = CameraOrientation.Automatic;
        static CameraSelection _selectedCamera;
        Action<string> _callback;

        /// <summary>
        /// The selected camera, front or back
        /// </summary>
        public CameraSelection SelectedCamera
        {
            get
            {
                return _selectedCamera;
            }
            set
            {
                if (_selectedCamera == value)
                    return;

                OpenCamera(value);
                SetTexture(_surface, _w, _h);
            }
        }

        /// <summary>
        /// The camera orientation
        /// </summary>
        public CameraOrientation CameraOrientation
        {
            get
            {
                return _cameraOrientation;
            }
            set
            {
                if (_cameraOrientation == value)
                    return;

                _cameraOrientation = value;
                SetCameraOrientation();
            }
        }

        /// <summary>
        /// Take a picture
        /// </summary>
        /// <param name="callback"></param>
        public void TakePicture(Action<string> callback)
        {
            if (_camera == null)
                return;

            _callback = callback;

            Android.Hardware.Camera.Parameters p = _camera.GetParameters();
            p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
            _camera.SetParameters(p);
            _camera.TakePicture(this, this, this);
        }

        /// <summary>
        /// Starts the camera
        /// </summary>
        /// <param name="selectedCamera">The selected camera, default: Back</param>
        /// <param name="orientation">the camera orientation, default: Automatic</param>
        public void StartCamera(CameraSelection selectedCamera = CameraSelection.Back, CameraOrientation orientation = Abstractions.CameraOrientation.Automatic)
        {
            if (_cameraOrientation == CameraOrientation.None)
                _cameraOrientation = orientation;

            if (_selectedCamera == CameraSelection.None)
                _selectedCamera = selectedCamera;

            _isCameraStarted = true;

            if (_surface != null)
                OpenCamera(_selectedCamera);
        }

        /// <summary>
        /// Stops the camera
        /// </summary>
        /// <param name="callback"></param>
        public void StopCamera()
        {
            CloseCamera();
        }

        #endregion

        private void Callback(string path)
        {
            var cb = _callback;
            _callback = null;

            cb(path);
        }

        //https://forums.xamarin.com/discussion/17625/custom-camera-takepicture
        void Camera.IPictureCallback.OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            File dataDir = Android.OS.Environment.ExternalStorageDirectory;
            if (data != null)
            {
                try
                {
                    var path = dataDir + "/" + pictureName;
                    //SaveFile(path, data);
                    RotateBitmap(path, data);
                    Callback(path);
                }
                catch (FileNotFoundException e)
                {
                    System.Console.Out.WriteLine(e.Message);
                }
                catch (IOException ie)
                {
                    System.Console.Out.WriteLine(ie.Message);
                }
            }
        }

        private void SaveFile(string path, byte[] data)
        {
            using (var outStream = new FileOutputStream(path))
            {
                outStream.Write(data);
                outStream.Close();
            }
        }

        void SetCameraOrientation()
        {
            // Google's camera orientation vs device orientation is all over the place, it changes per api version, per device type (phone/tablet) and per device brand
            // Credits: http://stackoverflow.com/questions/4645960/how-to-set-android-camera-orientation-properly
            if (_cameraInfo == null)
                return;

            Display display = _activity.WindowManager.DefaultDisplay;
            //var displayRotation = display.Rotation;
            int displayRotation = 0;

            switch (display.Rotation)
            {
                case SurfaceOrientation.Rotation0:
                    displayRotation = 0;
                    break;
                case SurfaceOrientation.Rotation90:
                    displayRotation = 90;
                    break;
                case SurfaceOrientation.Rotation180:
                    displayRotation = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    displayRotation = 270;
                    break;
                default:
                    break;
            }
            int correctedDisplayRotation;
            
            if (SelectedCamera == CameraSelection.Back)
            {
                _cameraHardwareRotation = MirrorOrientation(_cameraHardwareRotation); //(360 - _cameraHardwareRotation) % 360;
            }

            correctedDisplayRotation = (_cameraHardwareRotation + displayRotation) % 360;
            correctedDisplayRotation = MirrorOrientation(correctedDisplayRotation); //(360 - correctedDisplayRotation) % 360;  // compensate the mirror

            System.Console.WriteLine("displayRotation: {0}", displayRotation);
            System.Console.WriteLine("_cameraInfo.Orientation: {0}", _cameraInfo.Orientation);
            System.Console.WriteLine("_cameraHardwareOrientation: {0}", _cameraHardwareRotation);
            System.Console.WriteLine("correctedRotation: {0}", correctedDisplayRotation);

            _imageRotation = correctedDisplayRotation;

            if (SelectedCamera == CameraSelection.Back)
            {
                _imageRotation = MirrorOrientation(_imageRotation);
            }
            
            Android.Hardware.Camera.Parameters p = _camera.GetParameters();
            
            p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
            p.SetRotation(0);
            //p.SetRotation(_rotation);
            //p.SetRotation((_cameraInfo.Orientation + degrees) % 360);
            _camera.SetParameters(p);
            //_camera.SetDisplayOrientation(_rotation);
            _camera.SetDisplayOrientation(correctedDisplayRotation);

        }

        private int MirrorOrientation(int orientation)
        {
            return (360 - orientation) % 360;
        }

        /// <summary>
        /// Rotate the picture taken
        /// https://forums.xamarin.com/discussion/5409/photo-being-saved-in-landscape-not-portrait
        /// </summary>
        /// <param name="path"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        void RotateBitmap(string path, byte[] data)
        {
            try
            {
                //using (Android.Graphics.Bitmap picture = Android.Graphics.BitmapFactory.DecodeFile(path))
                using (Android.Graphics.Bitmap picture = data.ToBitmap())                
                using (Android.Graphics.Matrix mtx = new Android.Graphics.Matrix())
                {
                    ExifInterface exif = new ExifInterface(path);
                    string orientation = exif.GetAttribute(ExifInterface.TagOrientation);
                    var camOrientation = int.Parse(orientation);
                    
                    switch (_imageRotation)
                    {
                        case 0: // landscape
                            break;
                        case 90: // landscape upside down
                            mtx.PreRotate(270);
                            break;
                        case 180: // portrait
                            mtx.PreRotate(180);
                            break;
                        case 270: // portrait upside down
                            mtx.PreRotate(90);
                            break;                        
                    }

                    var maxSize = 1024;
                    double w = picture.Width;
                    double h = picture.Height;
                    if(picture.Width > maxSize || picture.Height > maxSize)
                    {
                        // set scaled width and height to prevent out of memory exception
                        double scaleFactor = (double)maxSize / (double)picture.Width;
                        if(picture.Height > picture.Width)
                            scaleFactor = (double)maxSize / picture.Height;                        

                        w = picture.Width * scaleFactor;
                        h = picture.Height * scaleFactor;                        
                    }

                    using (var scaledPiture = Android.Graphics.Bitmap.CreateScaledBitmap(picture, (int)w, (int)h, false))
                    using (var rotatedPiture = Android.Graphics.Bitmap.CreateBitmap(scaledPiture, 0, 0, (int)w, (int)h, mtx, false))
                    {
                        SaveFile(path, rotatedPiture.ToBytes());
                    }
                }
            }
            catch (Java.Lang.OutOfMemoryError e)
            {
                e.PrintStackTrace();
                throw;
            }
        }

        void Camera.IPreviewCallback.OnPreviewFrame(byte[] b, Android.Hardware.Camera c)
        {
        }

        void Camera.IShutterCallback.OnShutter()
        {
        }

        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h)
        {
            _surface = surface;
            _w = w;
            _h = h;

            if (!_isCameraStarted)
                return;

            OpenCamera(_selectedCamera);
            SetTexture(_surface, _w, _h);
        }

        private void SetTexture(Android.Graphics.SurfaceTexture surface, int w, int h)
        {
            if (_camera == null)
                return;

            SetCameraOrientation();

            this.LayoutParameters.Width = w;
            this.LayoutParameters.Height = h;

            try
            {
                //_camera.SetPreviewCallback(this);
                //_camera.Lock();
                _camera.SetPreviewTexture(surface);
                _camera.StartPreview();
            }
            catch (Java.IO.IOException ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            // ??
        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {
            // Fires whenever the surface change (moving the camera etc.)
        }

        public void OnSurfaceChanged(Android.Graphics.SurfaceTexture holder, int format, int w, int h)
        {
            // Now that the size is known, set up the camera parameters and begin
            // the preview.
            //Camera.Parameters parameters = mCamera.getParameters();
            //parameters.setPreviewSize(mPreviewSize.width, mPreviewSize.height);
            //requestLayout();
            //mCamera.setParameters(parameters);

            //// Important: Call startPreview() to start updating the preview surface.
            //// Preview must be started before you can take a picture.
            //mCamera.startPreview();
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            CloseCamera();
            return true;
        }

        private void OpenCamera(CameraSelection cameraSelection)
        {
            CloseCamera();

            int cameraCount = 0;
            _cameraInfo = new Camera.CameraInfo();
            cameraCount = Camera.NumberOfCameras;
            for (int camIdx = 0; camIdx < cameraCount; camIdx++)
            {
                Camera.GetCameraInfo(camIdx, _cameraInfo);
                if (_cameraInfo.Facing == cameraSelection.ToCameraFacing())
                {
                    try
                    {
                        
                        _camera = Camera.Open(camIdx);
                        _cameraHardwareRotation = _cameraInfo.Orientation;
                        //_cameraInfo = new Camera.CameraInfo();
                        //Android.Hardware.Camera.Parameters p = _camera.GetParameters();
                        //p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
                        //p.SetRotation(_rotation);
                        //_camera.SetParameters(p);

                        // SetPreviewCallback crashes when camera is released and called again
                        //_camera.SetPreviewCallback(this);
                        //_camera.Lock();

                        _selectedCamera = cameraSelection;
                        _isCameraStarted = true;
                    }
                    catch (Exception e)
                    {
                        CloseCamera();
                        Log.Error("CustomCameraView OpenCamera", e.Message);
                    }
                }
            }
        }

        private void CloseCamera()
        {
            if (_camera != null)
            {
                //_camera.Unlock();                
                _camera.StopPreview();
                //_camera.SetPreviewCallback(null);

                _camera.Release();
                _camera = null;
                _isCameraStarted = false;
            }
        }
    }
}