using DaySign.Script.Expand;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DaySign
{
    public enum FaceType
    {
        FD //人脸检测（是否存在人脸）
    }

    public struct InitialData
    {
        public string appId; 
        public string sdkKey;
        public int detectSizeMB;
        public int nScale;
        public int nMaxFaceNum;
    }

    abstract public class Face
    {
        public delegate int InitialFaceEngineAPI(string appId, string sdkKey, IntPtr pMem, int lMemSize, ref IntPtr pEngine, int iOrientPriority, int nScale, int nMaxFaceNum);

        public delegate int UninitialFaceEngineAPI(IntPtr pEngine);

        public IntPtr detectEngine; //引擎指针
        //public abstract IntPtr DetectEngine { get; set; }

        protected void InitialFaceEngine(InitialData initial, InitialFaceEngineAPI initialAPI)
        {
            detectEngine = IntPtr.Zero;
            int detectSize = initial.detectSizeMB * 1024 * 1024;
            IntPtr pMen = Marshal.AllocHGlobal(detectSize);
            int i = initialAPI(initial.appId, initial.sdkKey, pMen, detectSize, ref detectEngine, (int)AFD_FSDK_OrientPriority.AFD_FSDK_OPF_0_HIGHER_EXT, initial.nScale, initial.nMaxFaceNum);
            if (i != 0)
            {
                MessageBox.Show("创建引擎失败！" + i);
            }
        }

        protected void InitialFaceEngine(string appId, string sdkKey, InitialFaceEngineAPI initialAPI)
        {
            InitialData data = new InitialData()
            {
                appId = appId,
                sdkKey = sdkKey,
                detectSizeMB = 40,
                nScale = 16,
                nMaxFaceNum = 1
            };
            InitialFaceEngine(data, initialAPI);
        }

        abstract protected InitialFaceEngineAPI InitialAPI { get; }

        public void InitialFaceEngine(InitialData initial)
        {
            InitialFaceEngine(initial, InitialAPI);
        }

        public void InitialFaceEngine(string appId, string sdkKey)
        {
            InitialFaceEngine(appId, sdkKey, InitialAPI);
        }

        abstract protected UninitialFaceEngineAPI UninitialAPI { get; }

        public void UninitialFaceEngine(IntPtr pEngine)
        {
            UninitialAPI(pEngine);
        }

        protected static byte[] BitmapToBmp(Bitmap image, out int width, out int height, out int pitch)
        {
            //将Bitmap锁定到系统内存中,获得BitmapData
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            //位图中第一个像素数据的地址。它也可以看成是位图中的第一个扫描行
            IntPtr ptr = data.Scan0;
            //定义数组长度
            int soureBitArrayLength = data.Height * Math.Abs(data.Stride); //Stride：内存对齐后的宽度
            byte[] sourceBitArray = new byte[soureBitArrayLength];
            //将bitmap中的内容拷贝到ptr_bgr数组中
            Marshal.Copy(ptr, sourceBitArray, 0, soureBitArrayLength);
            width = data.Width;
            height = data.Height;
            pitch = Math.Abs(data.Stride);
            int line = width * 3;
            int bgr_len = line * height;
            byte[] destBitArray = new byte[bgr_len];
            for (int i = 0; i < height; ++i)
            {
                Array.Copy(sourceBitArray, i * pitch, destBitArray, i * line, line);
            }
            pitch = line;
            image.UnlockBits(data);
            return destBitArray;
        }

    }

    //检查人脸
    public class Face_FD : Face
    {
        protected override InitialFaceEngineAPI InitialAPI => new InitialFaceEngineAPI(FaceAPI.InitialFaceEngine_FD);

        protected override UninitialFaceEngineAPI UninitialAPI => new UninitialFaceEngineAPI(FaceAPI.AFD_FSDK_UninitialFaceEngine);

        //检查是否存在人脸，imageDataPtr必须在offInputPtr用完后释放掉
        public bool CheckFace(Bitmap bitmap, out AFD_FSDK_FACERES faceRes, out IntPtr offInputPtr, out IntPtr imageDataPtr)
        {
            byte[] imageData = BitmapToBmp(bitmap, out int width, out int height, out int pitch);
            
            imageDataPtr = Marshal.AllocHGlobal(imageData.Length);
            Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

            ASVLOFFSCREEN offInput = new ASVLOFFSCREEN();
            offInput.u32PixelArrayFormat = 513;
            offInput.ppu8Plane = new IntPtr[4];
            offInput.ppu8Plane[0] = imageDataPtr;
            offInput.i32Width = width;
            offInput.i32Height = height;
            offInput.pi32Pitch = new int[4];
            offInput.pi32Pitch[0] = pitch;
            offInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(offInput));
            Marshal.StructureToPtr(offInput, offInputPtr, false);

            faceRes = new AFD_FSDK_FACERES();
            IntPtr faceResPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceRes));
            int detectResult = FaceAPI.AFD_FSDK_StillImageFaceDetection(detectEngine, offInputPtr, ref faceResPtr);

            faceRes = (AFD_FSDK_FACERES)Marshal.PtrToStructure(faceResPtr, typeof(AFD_FSDK_FACERES));
            MRECT rect = (MRECT)Marshal.PtrToStructure(faceRes.rcFace, typeof(MRECT));
            
            bool ret = faceRes.nFace > 0;

            imageData = null;

            //Marshal.FreeHGlobal(imageDataPtr); //这个指针内存泄漏了

            //Marshal.FreeHGlobal(faceResPtr); 
            //GC.Collect();
            return ret;
        }

        public bool CheckFace(Bitmap bitmap)
        {
            return CheckFace(bitmap, out AFD_FSDK_FACERES faceRes, out IntPtr offInputPtr, out IntPtr imageDataPtr);
        }

        public bool CheckFace(Bitmap bitmap, out AFD_Face face)
        {
            bool ret = CheckFace(bitmap, out AFD_FSDK_FACERES faceRes, out IntPtr offInputPtr, out IntPtr imageDataPtr);
            face = faceRes.DeIntPtr();
            return ret;
        }


    }

    //获取人脸信息和匹配人脸
    public class Face_FR : Face
    {
        protected override InitialFaceEngineAPI InitialAPI => new InitialFaceEngineAPI(FaceAPI.InitialFaceEngine_FR);

        protected override UninitialFaceEngineAPI UninitialAPI => new UninitialFaceEngineAPI(FaceAPI.AFR_FSDK_UninitialEngine);

        public byte[] GetFaceData(AFD_FSDK_FACERES faceRes, IntPtr offInputPtr)
        {
            AFR_FSDK_FaceInput faceinput = new AFR_FSDK_FaceInput();
            faceinput.lOrient = (int)Marshal.PtrToStructure(faceRes.lfaceOrient, typeof(int));
            MRECT rect = (MRECT)Marshal.PtrToStructure(faceRes.rcFace, typeof(MRECT));
            faceinput.rcFace = rect;

            IntPtr faceInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceinput));
            Marshal.StructureToPtr(faceinput, faceInputPtr, false);

            AFR_FSDK_FaceModel faceModel = new AFR_FSDK_FaceModel();
            IntPtr faceModelPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceModel));

            int ret = FaceAPI.AFR_FSDK_ExtractFRFeature
                (detectEngine, offInputPtr,
                faceInputPtr, faceModelPtr);

            if (ret != 0) //返回值为0代表获取成功
            {
                Log.AddLog("获取不到人脸信息。");
                return null;
            }

            faceModel = (AFR_FSDK_FaceModel)Marshal.PtrToStructure(faceModelPtr, typeof(AFR_FSDK_FaceModel));

            byte[] byteData = new byte[faceModel.lFeatureSize];
            Marshal.Copy(faceModel.pbFeature, byteData, 0, faceModel.lFeatureSize);

            Marshal.FreeHGlobal(faceModelPtr);
            Marshal.FreeHGlobal(faceInputPtr);

            return byteData;
        }


        [Obsolete]
        public byte[] GetFaceData(Bitmap bitmap, Face_FD face_FD)
        {
            bool isFace = face_FD.CheckFace(bitmap, out AFD_FSDK_FACERES faceRes, out IntPtr offInputPtr, out IntPtr imageDataPtr);
            if (!isFace)
            {
                Error.Log(ErrorType.inputError);
                return null;
            }
                
            AFR_FSDK_FaceInput faceinput = new AFR_FSDK_FaceInput();
            faceinput.lOrient = (int)Marshal.PtrToStructure(faceRes.lfaceOrient, typeof(int));
            MRECT rect = (MRECT)Marshal.PtrToStructure(faceRes.rcFace, typeof(MRECT));
            faceinput.rcFace = rect;

            IntPtr faceInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceinput));
            Marshal.StructureToPtr(faceinput, faceInputPtr, false);
            
            AFR_FSDK_FaceModel faceModel = new AFR_FSDK_FaceModel();
            IntPtr faceModelPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceModel));

            int ret = FaceAPI.AFR_FSDK_ExtractFRFeature(detectEngine, offInputPtr, 
                faceInputPtr, faceModelPtr);

            if (ret != 0) //返回值为0代表获取成功
            {
                Log.AddLog("获取不到人脸信息。");
                return null; 
            }

            faceModel = (AFR_FSDK_FaceModel)Marshal.PtrToStructure(faceModelPtr, typeof(AFR_FSDK_FaceModel));
            Marshal.FreeHGlobal(faceModelPtr);

            byte[] byteData = new byte[faceModel.lFeatureSize];
            Marshal.Copy(faceModel.pbFeature, byteData, 0, faceModel.lFeatureSize);

            Marshal.FreeHGlobal(faceInputPtr);

            return byteData;
        }

        public float CompareFace(byte[] data, byte[] beData) //返回相似系数
        {
            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);
            AFR_FSDK_FaceModel faceModel = new AFR_FSDK_FaceModel
            {
                lFeatureSize = data.Length,
                pbFeature = dataPtr
            };

            IntPtr beDataPtr = Marshal.AllocHGlobal(beData.Length);
            Marshal.Copy(beData, 0, beDataPtr, beData.Length);
            AFR_FSDK_FaceModel beFaceModel = new AFR_FSDK_FaceModel
            {
                lFeatureSize = beData.Length,
                pbFeature = beDataPtr
            };

            IntPtr firstPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceModel));
            Marshal.StructureToPtr(faceModel, firstPtr, false);

            IntPtr secondPtr = Marshal.AllocHGlobal(Marshal.SizeOf(beFaceModel));
            Marshal.StructureToPtr(beFaceModel, secondPtr, false);

            float result = 0; //大约0.55，是同个人
            int ret = FaceAPI.AFR_FSDK_FacePairMatching(detectEngine, firstPtr, secondPtr, ref result);

            Marshal.FreeHGlobal(dataPtr);
            Marshal.FreeHGlobal(beDataPtr);
            Marshal.FreeHGlobal(firstPtr);
            Marshal.FreeHGlobal(secondPtr);

            return result;
        }
    }
}
