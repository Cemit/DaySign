#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <Windows.h>
#include "arcsoft_fsdk_face_detection.h"
#include "merror.h"

#pragma comment(lib,"libarcsoft_fsdk_face_detection.lib")

#define WORKBUF_SIZE        (40*1024*1024)
#define INPUT_IMAGE_PATH "sample.bmp"
#define APPID		""			//APPID
#define SDKKey		""			//SDKKey

bool readBmp24(const char* path, uint8_t **imageData, int *pWidth, int *pHeight)
{
	if (path == NULL || imageData == NULL || pWidth == NULL || pHeight == NULL)
	{
		return false;
	}
	FILE *fp = fopen(path, "rb");
	if (fp == NULL)
	{
		return false;
	}
	fseek(fp, sizeof(BITMAPFILEHEADER), 0);
	BITMAPINFOHEADER head;
	fread(&head, sizeof(BITMAPINFOHEADER), 1, fp);
	*pWidth = head.biWidth;
	*pHeight = head.biHeight;
	int biBitCount = head.biBitCount;
	if (24 == biBitCount)
	{
		int lineByte = ((*pWidth) * biBitCount / 8 + 3) / 4 * 4;
		*imageData = (uint8_t *)malloc(lineByte * (*pHeight));
		uint8_t * data = (uint8_t *)malloc(lineByte * (*pHeight));
		fseek(fp, 54, SEEK_SET);
		fread(data, 1, lineByte * (*pHeight), fp);
		for (int i = 0; i < *pHeight; i++)
		{
			for (int j = 0; j < *pWidth; j++)
			{
				memcpy((*imageData) + i * (*pWidth) * 3 + j * 3, data + (((*pHeight) - 1) - i) * lineByte + j * 3, 3);
			}
		}
		free(data);
	}
	else
	{
		fclose(fp);
		return false;
	}
	fclose(fp);
	return true;
}
int main()
{
	/* 初始化引擎和变量 */
	MRESULT nRet = MERR_UNKNOWN;
	MHandle hEngine = nullptr;
	MInt32 nScale = 16;
	MInt32 nMaxFace = 10;
	MByte *pWorkMem = (MByte *)malloc(WORKBUF_SIZE);
	if (pWorkMem == nullptr)
	{
		return -1;
	}
	nRet = AFD_FSDK_InitialFaceEngine(APPID, SDKKey, pWorkMem, WORKBUF_SIZE, &hEngine, AFD_FSDK_OPF_0_HIGHER_EXT, nScale, nMaxFace);
	if (nRet != MOK)
	{
		return -1;
	}
	/* 打印版本信息 */
	const AFD_FSDK_Version * pVersionInfo = nullptr;
	pVersionInfo = AFD_FSDK_GetVersion(hEngine);
	fprintf(stdout, "%d %d %d %d\n", pVersionInfo->lCodebase, pVersionInfo->lMajor, pVersionInfo->lMinor, pVersionInfo->lBuild);
	fprintf(stdout, "%s\n", pVersionInfo->Version);
	fprintf(stdout, "%s\n", pVersionInfo->BuildDate);
	fprintf(stdout, "%s\n", pVersionInfo->CopyRight);

	/* 读取静态图片信息，并保存到ASVLOFFSCREEN结构体 （以ASVL_PAF_RGB24_B8G8R8格式为例） */
	ASVLOFFSCREEN offInput = { 0 };
	offInput.u32PixelArrayFormat = ASVL_PAF_RGB24_B8G8R8;
	offInput.ppu8Plane[0] = nullptr;
	readBmp24(INPUT_IMAGE_PATH, (uint8_t**)&offInput.ppu8Plane[0], &offInput.i32Width, &offInput.i32Height);
	if (!offInput.ppu8Plane[0])
	{
		fprintf(stderr, "Fail to ReadBmp(%s)\n", INPUT_IMAGE_PATH);
		AFD_FSDK_UninitialFaceEngine(hEngine);
		free(pWorkMem);
		return -1;
	}
	else
	{
		fprintf(stdout, "Picture width : %d , height : %d \n", offInput.i32Width, offInput.i32Height);
	}
	offInput.pi32Pitch[0] = offInput.i32Width * 3;

	/* 人脸检测 */
	LPAFD_FSDK_FACERES	FaceRes = nullptr;
	nRet = AFD_FSDK_StillImageFaceDetection(hEngine, &offInput, &FaceRes);
	if (nRet != MOK)
	{
		fprintf(stderr, "Face Detection failed, error code: %d\n", nRet);
	}
	else
	{
		fprintf(stdout, "The number of face: %d\n", FaceRes->nFace);
		for (int i = 0; i < FaceRes->nFace; ++i)
		{
			fprintf(stdout, "Face[%d]: rect[%d,%d,%d,%d], Face orient: %d\n", i, FaceRes->rcFace[i].left, FaceRes->rcFace[i].top, FaceRes->rcFace[i].right, FaceRes->rcFace[i].bottom, FaceRes->lfaceOrient[i]);
		}
	}

	/* 释放引擎和内存 */
	nRet = AFD_FSDK_UninitialFaceEngine(hEngine);
	if (nRet != MOK)
	{
		fprintf(stderr, "UninitialFaceEngine failed , errorcode is %d \n", nRet);
	}
	free(offInput.ppu8Plane[0]);
	free(pWorkMem);
	return 0;
}

