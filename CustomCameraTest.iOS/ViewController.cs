﻿using CoreGraphics;
using System;
using UIKit;
using AVFoundation;
using Foundation;
using Plugin.CustomCamera;
using Plugin.CustomCamera.Abstractions;

namespace CustomCameraTest.iOS
{
    public partial class ViewController : UIViewController
    {
        int yPos;
        const int defaultSize = 300;
        UIImageView _previewImage;
        UIButton _takePictureButton;
        const string ResetCamera = "Reset Camera";

        public ViewController(IntPtr handle)
            : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            yPos = (int)View.Bounds.Height - 60;

            // CustomCamera Plugin EXAMPLE
            var _customCameraView = (UIView)CrossCustomCamera.Current.CustomCameraView;
            _customCameraView.Frame = View.Frame;
            Add(_customCameraView);
            CrossCustomCamera.Current.CustomCameraView.Start(CameraSelection.Front);

            _takePictureButton = AddButton("Take Picture");
            _takePictureButton.TouchUpInside += takePictureButton_TouchUpInside;
            
            _previewImage = new UIImageView(new CGRect(View.Bounds.Width / 2 - 50, View.Bounds.Height - 250, 100, 180));
            _previewImage.Hidden = true;
            Add(_previewImage);
        }
                   
        void takePictureButton_TouchUpInside(object sender, EventArgs e)
        {
            // UIImagePickerController EXAMPLE
            //InitializeUIImagePickerController();

            // AVFoundation EXAMPLE
            //InitializeAVFoundation();

            // CustomCamera plugin EXAMPLE
            if (_takePictureButton.Title(UIControlState.Normal) == ResetCamera)
            {
                // Reset camera
                _previewImage.Hidden = true;
                CrossCustomCamera.Current.CustomCameraView.Reset();
                //_pictureTaken = false;
                _takePictureButton.SetTitle("Take Picture", UIControlState.Normal);
                return;
            }

            // Take picture
            CrossCustomCamera.Current.CustomCameraView.TakePicture((path) =>
            {
                _previewImage.Image = new UIImage(path);
                _previewImage.Hidden = false;
                _takePictureButton.SetTitle(ResetCamera, UIControlState.Normal);
            });
        }

        public UIButton AddButton(string title, int height = 40)
        {
            var button = new UIButton(GetFrame(height));
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(new UIColor(1, 0, 0, 1), UIControlState.Normal);
            Add(button);

            return button;
        }

        public CGRect GetFrame(int height = 40, int width = defaultSize)
        {
            var rect = new CGRect(40, yPos, width, height);
            yPos += height;

            return rect;
        }

        //#region UIImagePickerController

        //UIImagePickerController _imagePicker;
        //bool _animated = false;

        //void InitializeUIImagePickerController()
        //{
        //    // UIImagePickerController EXAMPLE
        //    _imagePicker = new UIImagePickerController();
        //    _imagePicker.SourceType = UIImagePickerControllerSourceType.Camera;
        //    //imagePicker.CameraOverlayView.Frame = new CoreGraphics.CGRect(20, 20, 300, 300);
        //    _imagePicker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;
        //    _imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera);
        //    _imagePicker.CameraDevice = UIImagePickerControllerCameraDevice.Front;

        //    var captureButton = AddButton("Capture");
        //    captureButton.TouchUpInside += captureButton_TouchUpInside;
        //    _imagePicker.CameraOverlayView.Add(captureButton);
        //    _imagePicker.ShowsCameraControls = false;
        //    _imagePicker.Canceled += _imagePicker_Canceled;
        //    //_imagePicker.TakePicture += _imagePicker_FinishedPickingImage;

        //    PresentViewController(_imagePicker, _animated, null);
        //    //NavigationController.PresentModalViewController(imagePicker, true);
        //    //Add(_imagePicker);
        //}

        //void _imagePicker_FinishedPickingImage(object sender, UIImagePickerImagePickedEventArgs e)
        //{
        //    _imagePicker.DismissModalViewController(_animated);
        //}

        //void _imagePicker_Canceled(object sender, EventArgs e)
        //{
        //    _imagePicker.DismissModalViewController(_animated);
        //}

        //void captureButton_TouchUpInside(object sender, EventArgs e)
        //{
        //    _imagePicker.TakePicture();
        //}
                
        //public override void DidReceiveMemoryWarning()
        //{
        //    base.DidReceiveMemoryWarning();
        //    // Release any cached data, images, etc that aren't in use.
        //}

        //#endregion

        //#region AVFoundation

        //AVCaptureSession _captureSession;
        //AVCaptureDeviceInput _captureDeviceInput;
        //AVCaptureStillImageOutput _stillImageOutput;
        //UIView _liveCameraStream;
        //UIButton _takePictureButton;
        //UIButton _toggleCameraButton;
        //UIButton _toggleFlashButton;
        //UIImageView _picture;
        //bool _resetCamera = false;

        //void InitializeAVFoundation()
        //{
        //    try
        //    {
        //        SetupUserInterface();
        //        SetupEventHandlers();
        //        SetupLiveCameraStream();
        //        AuthorizeCameraUse();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(@"			ERROR: ", ex.Message);
        //    }
        //}

        //void SetupUserInterface()
        //{
        //    var centerButtonX = View.Bounds.GetMidX() - 35f;
        //    var topLeftX = View.Bounds.X + 25;
        //    var topRightX = View.Bounds.Right - 65;
        //    var bottomButtonY = View.Bounds.Bottom - 150;
        //    var topButtonY = View.Bounds.Top + 15;
        //    var buttonWidth = 100;
        //    var buttonHeight = 70;

        //    _liveCameraStream = new UIView() { Frame = new CGRect(0f, 0f, View.Bounds.Width, View.Bounds.Height) };
        //    _picture = new UIImageView() { Frame = new CGRect(0f, 0f, View.Bounds.Width, View.Bounds.Height) };
        //    _picture.Hidden = true;

        //    _takePictureButton = new UIButton() { Frame = new CGRect(centerButtonX, bottomButtonY, buttonWidth, buttonHeight) };
        //    _takePictureButton.SetTitle("Take Picture", UIControlState.Normal);

        //    _toggleCameraButton = new UIButton() { Frame = new CGRect(topRightX, topButtonY + 5, buttonWidth, 26) };
        //    _toggleCameraButton.SetTitle("Toggle Camera", UIControlState.Normal);

        //    _toggleFlashButton = new UIButton() { Frame = new CGRect(topLeftX, topButtonY, buttonWidth, 37) };
        //    _toggleFlashButton.SetTitle("Toggle flash", UIControlState.Normal);

        //    View.Add(_liveCameraStream);
        //    View.Add(_picture);
        //    View.Add(_takePictureButton);
        //    View.Add(_toggleCameraButton);
        //    View.Add(_toggleFlashButton);
        //}

        //void SetupEventHandlers()
        //{
        //    _takePictureButton.TouchUpInside += (object sender, EventArgs e) =>
        //    {
        //        TakePicture();
        //    };

        //    _toggleCameraButton.TouchUpInside += (object sender, EventArgs e) =>
        //    {
        //        ToggleFrontBackCamera();
        //    };

        //    _toggleFlashButton.TouchUpInside += (object sender, EventArgs e) =>
        //    {
        //        ToggleFlash();
        //    };
        //}

        //async void TakePicture()
        //{
        //    if (_resetCamera)
        //    {
        //        SetPicture("Take Picture", null);
        //        _resetCamera = !_resetCamera;
        //        return;
        //    }

        //    var videoConnection = _stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
        //    var sampleBuffer = await _stillImageOutput.CaptureStillImageTaskAsync(videoConnection);
        //    var jpegImage = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);

        //    var picture = new UIImage(jpegImage);
        //    picture.SaveToPhotosAlbum((image, error) =>
        //    {

        //        Console.Error.WriteLine(@"				Error: ", error);
        //    });

        //    //picture.SaveToPhotosAlbum((image, error) => { });

        //    SetPicture("Reset Camera", picture);
        //    _resetCamera = !_resetCamera;
        //}

        //private void SetPicture(string title, UIImage picture)
        //{
        //    _picture.Image = picture;
        //    _picture.Hidden = false;
        //    _takePictureButton.SetTitle(title, UIControlState.Normal);
        //}

        //void ToggleFrontBackCamera()
        //{
        //    var devicePosition = _captureDeviceInput.Device.Position;
        //    if (devicePosition == AVCaptureDevicePosition.Front)
        //    {
        //        devicePosition = AVCaptureDevicePosition.Back;
        //    }
        //    else
        //    {
        //        devicePosition = AVCaptureDevicePosition.Front;
        //    }

        //    var device = GetCameraForOrientation(devicePosition);
        //    ConfigureCameraForDevice(device);

        //    _captureSession.BeginConfiguration();
        //    _captureSession.RemoveInput(_captureDeviceInput);
        //    _captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
        //    _captureSession.AddInput(_captureDeviceInput);
        //    _captureSession.CommitConfiguration();
        //}

        //void ToggleFlash()
        //{
        //    var device = _captureDeviceInput.Device;

        //    var error = new NSError();
        //    if (device.HasFlash)
        //    {
        //        if (device.FlashMode == AVCaptureFlashMode.On)
        //        {
        //            device.LockForConfiguration(out error);
        //            device.FlashMode = AVCaptureFlashMode.Off;
        //            device.UnlockForConfiguration();
        //            _toggleFlashButton.SetBackgroundImage(UIImage.FromFile("NoFlashButton.png"), UIControlState.Normal);
        //        }
        //        else
        //        {
        //            device.LockForConfiguration(out error);
        //            device.FlashMode = AVCaptureFlashMode.On;
        //            device.UnlockForConfiguration();
        //            _toggleFlashButton.SetBackgroundImage(UIImage.FromFile("FlashButton.png"), UIControlState.Normal);
        //        }
        //    }
        //}

        //AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
        //{
        //    var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

        //    foreach (var device in devices)
        //    {
        //        if (device.Position == orientation)
        //        {
        //            return device;
        //        }
        //    }
        //    return null;
        //}

        //void SetupLiveCameraStream()
        //{
        //    _captureSession = new AVCaptureSession();

        //    var viewLayer = _liveCameraStream.Layer;
        //    var videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
        //    {
        //        Frame = _liveCameraStream.Bounds
        //    };
        //    _liveCameraStream.Layer.AddSublayer(videoPreviewLayer);

        //    var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);
        //    ConfigureCameraForDevice(captureDevice);
        //    _captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);

        //    var dictionary = new NSMutableDictionary();
        //    dictionary[AVVideo.CodecKey] = new NSNumber((int)AVVideoCodec.JPEG);
        //    _stillImageOutput = new AVCaptureStillImageOutput()
        //    {
        //        OutputSettings = new NSDictionary()
        //    };

        //    _captureSession.AddOutput(_stillImageOutput);
        //    _captureSession.AddInput(_captureDeviceInput);
        //    _captureSession.StartRunning();
        //}

        //void ConfigureCameraForDevice(AVCaptureDevice device)
        //{
        //    var error = new NSError();
        //    if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
        //    {
        //        device.LockForConfiguration(out error);
        //        device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
        //        device.UnlockForConfiguration();
        //    }
        //    else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
        //    {
        //        device.LockForConfiguration(out error);
        //        device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
        //        device.UnlockForConfiguration();
        //    }
        //    else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
        //    {
        //        device.LockForConfiguration(out error);
        //        device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
        //        device.UnlockForConfiguration();
        //    }
        //}

        //async void AuthorizeCameraUse()
        //{
        //    var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
        //    if (authorizationStatus != AVAuthorizationStatus.Authorized)
        //    {
        //        await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
        //    }
        //}

        //#endregion

    }
}

