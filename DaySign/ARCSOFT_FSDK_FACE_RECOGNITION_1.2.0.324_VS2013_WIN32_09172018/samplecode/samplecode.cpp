#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <Windows.h>
#include "arcsoft_fsdk_face_recognition.h"
#include "merror.h"

#pragma comment(lib,"libarcsoft_fsdk_face_recognition.lib")

#define WORKBUF_SIZE        (40*1024*1024)
#define INPUT_IMAGE1_PATH "sample1.bmp"
#define INPUT_IMAGE2_PATH "sample2.bmp"
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
	/* ��ʼ������ͱ��� */
	MRESULT nRet = MERR_UNKNOWN;
	MHandle hEngine = nullptr;
	MInt32 nScale = 16;
	MInt32 nMaxFace = 10;
	MByte *pWorkMem = (MByte *)malloc(WORKBUF_SIZE);
	if (pWorkMem == nullptr)
	{
		return -1;
	}
	nRet = AFR_FSDK_InitialEngine(APPID, SDKKey, pWorkMem, WORKBUF_SIZE, &hEngine);
	if (nRet != MOK)
	{
		return -1;
	}
	/* ��ӡ�汾��Ϣ */
	const AFR_FSDK_Version * pVersionInfo = nullptr;
	pVersionInfo = AFR_FSDK_GetVersion(hEngine);
	fprintf(stdout, "%d %d %d %d %d\n", pVersionInfo->lCodebase, pVersionInfo->lMajor, pVersionInfo->lMinor, pVersionInfo->lBuild, pVersionInfo->lFeatureLevel);
	fprintf(stdout, "%s\n", pVersionInfo->Version);
	fprintf(stdout, "%s\n", pVersionInfo->BuildDate);
	fprintf(stdout, "%s\n", pVersionInfo->CopyRight);

	/* ��ȡ��һ�ž�̬ͼƬ��Ϣ�������浽ASVLOFFSCREEN�ṹ�� ����ASVL_PAF_RGB24_B8G8R8��ʽΪ���� */
	ASVLOFFSCREEN offInput1 = { 0 };
	offInput1.u32PixelArrayFormat = ASVL_PAF_RGB24_B8G8R8;
	offInput1.ppu8Plane[0] = nullptr;
	readBmp24(INPUT_IMAGE1_PATH, (uint8_t**)&offInput1.ppu8Plane[0], &offInput1.i32Width, &offInput1.i32Height);
	if (!offInput1.ppu8Plane[0])
	{
		fprintf(stderr, "fail to ReadBmp(%s)\n", INPUT_IMAGE1_PATH);
		AFR_FSDK_UninitialEngine(hEngine);
		free(pWorkMem);
		return -1;
	}
	offInput1.pi32Pitch[0] = offInput1.i32Width * 3;
	AFR_FSDK_FACEMODEL faceModels1 = { 0 };
	{
		AFR_FSDK_FACEINPUT faceInput;
		//��һ��������Ϣͨ��face detection\face tracking���
		faceInput.lOrient = AFR_FSDK_FOC_0;//��������
		//������λ��
		faceInput.rcFace.left = 346;
		faceInput.rcFace.top = 58;
		faceInput.rcFace.right = 440;
		faceInput.rcFace.bottom = 151;
		//��ȡ��һ����������
		AFR_FSDK_FACEMODEL LocalFaceModels = { 0 };
		nRet = AFR_FSDK_ExtractFRFeature(hEngine, &offInput1, &faceInput, &LocalFaceModels);
		if (nRet != MOK)
		{
			fprintf(stderr, "fail to Extract 1st FR Feature, error code: %d\n", nRet);
		}
		/* ��������������� */
		faceModels1.lFeatureSize = LocalFaceModels.lFeatureSize;
		faceModels1.pbFeature = (MByte*)malloc(faceModels1.lFeatureSize);
		memcpy(faceModels1.pbFeature, LocalFaceModels.pbFeature, faceModels1.lFeatureSize);
	}
	/* ��ȡ�ڶ��ž�̬ͼƬ��Ϣ�������浽ASVLOFFSCREEN�ṹ�� ����ASVL_PAF_RGB24_B8G8R8��ʽΪ���� */
	ASVLOFFSCREEN offInput2 = { 0 };
	offInput2.u32PixelArrayFormat = ASVL_PAF_RGB24_B8G8R8;
	offInput2.ppu8Plane[0] = nullptr;
	readBmp24(INPUT_IMAGE2_PATH, (uint8_t**)&offInput2.ppu8Plane[0], &offInput2.i32Width, &offInput2.i32Height);
	if (!offInput2.ppu8Plane[0])
	{
		fprintf(stderr, "fail to ReadBmp(%s)\n", INPUT_IMAGE2_PATH);
		free(offInput1.ppu8Plane[0]);
		AFR_FSDK_UninitialEngine(hEngine);
		free(pWorkMem);
		return -1;
	}
	offInput2.pi32Pitch[0] = offInput2.i32Width * 3;
	AFR_FSDK_FACEMODEL faceModels2 = { 0 };
	{
		AFR_FSDK_FACEINPUT faceInput;
		//�ڶ���������Ϣͨ��face detection\face tracking���
		faceInput.lOrient = AFR_FSDK_FOC_0;//��������
		//������λ��
		faceInput.rcFace.left = 122;
		faceInput.rcFace.top = 76;
		faceInput.rcFace.right = 478;
		faceInput.rcFace.bottom = 432;
		//��ȡ�ڶ�����������
		AFR_FSDK_FACEMODEL LocalFaceModels = { 0 };
		nRet = AFR_FSDK_ExtractFRFeature(hEngine, &offInput2, &faceInput, &LocalFaceModels);
		if (nRet != MOK)
		{
			fprintf(stderr, "fail to Extract 2nd FR Feature, error code: %d\n", nRet);
		}
		/* ��������������� */
		faceModels2.lFeatureSize = LocalFaceModels.lFeatureSize;
		faceModels2.pbFeature = (MByte*)malloc(faceModels2.lFeatureSize);
		memcpy(faceModels2.pbFeature, LocalFaceModels.pbFeature, faceModels2.lFeatureSize);
	}
	/* �Ա�����������������ñȶԽ�� */
	MFloat  fSimilScore = 0.0f;
	nRet = AFR_FSDK_FacePairMatching(hEngine, &faceModels1, &faceModels2, &fSimilScore);
	if (nRet == MOK)
	{
		fprintf(stdout, "fSimilScore =  %f\n", fSimilScore);
	}
	else
	{
		fprintf(stderr, "FacePairMatching failed , errorcode is %d \n", nRet);
	}
	/* �ͷ�������ڴ� */
	nRet = AFR_FSDK_UninitialEngine(hEngine);
	if (nRet != MOK)
	{
		fprintf(stderr, "UninitialFaceEngine failed , errorcode is %d \n", nRet);
	}
	free(offInput1.ppu8Plane[0]);
	free(offInput2.ppu8Plane[0]);
	free(faceModels1.pbFeature);
	free(faceModels2.pbFeature);
	free(pWorkMem);
	return 0;
}

