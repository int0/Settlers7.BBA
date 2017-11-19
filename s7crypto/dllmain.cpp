// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

static const UINT HeaderKey[4]     = { 0x2365E3F2, 0xD5442128, 0x19903D44, 0x61837531 };
static const UINT IndexTableKey[4] = { 0x7CA59982, 0x42D53C9D, 0x8AE60A45, 0x916E6F48 };
static const UINT FileHeaderKey[4] = { 0x4D9BE24D, 0x968D0A3A, 0x5ACC530F, 0x67196A8C };
static const UINT TableKey[4]      = { 0x4994DA61, 0x9E7E321A, 0x3C4C2B78, 0x9A21B5C4 };
static const UINT HKey[4]          = { 0x4995DA61, 0x9F7E321A, 0x3C4C2A78, 0x9A31B5C4 };

BOOL IsDemoMode = FALSE;

static UINT TableSeed;
static UINT HeaderSeed;
static UINT HeaderNumberOfPasses;
static UINT TableNumberOfPasses;

#define DemoHeaderNumberOfPasses 6
#define RetailHeaderNumberOfPasses 7

#define DemoHeaderSeed 0x436C97A3
#define RetailHeaderSeed 0x496C97A3

#define DemoTableNumberOfPasses 9
#define RetailTableNumberOfPasses 8

#define DemoTableSeed 0x5B50C3DE
#define RetailTableSeed 0x5B54C3DE

void __stdcall SetMode( BOOL IsDemo )
{
	if ( IsDemo )
	{
		TableSeed = DemoTableSeed;
		TableNumberOfPasses = DemoTableNumberOfPasses;
		
		HeaderSeed = DemoHeaderSeed;
		HeaderNumberOfPasses = DemoHeaderNumberOfPasses;
		IsDemoMode = TRUE;
	}
	else
	{
		TableSeed = RetailTableSeed;
		TableNumberOfPasses = RetailTableNumberOfPasses;

		HeaderSeed = RetailHeaderSeed;
		HeaderNumberOfPasses = RetailHeaderNumberOfPasses;
		IsDemoMode = FALSE;
	}
}

bool __cdecl DecryptHeaderWithKey(PUINT Header, UINT HeaderSizeInBytes, PUINT BBAHeader, UINT BBAHeaderSizeInBytes)
{
	int v17;
	unsigned int i, j;
	unsigned int v23;
	unsigned int NumberOfPasses = HeaderNumberOfPasses;
	UINT HeaderSizeInDWords;
	

	if ( Header && !(HeaderSizeInBytes & 3) && BBAHeader && BBAHeaderSizeInBytes >= 0x10 )
	{
		HeaderSizeInDWords = HeaderSizeInBytes >> 2;
		if ( (signed int)(HeaderSizeInBytes >> 2) > 1 )
		{
			v23 = *Header;
			
			for ( i = HeaderSeed * NumberOfPasses; i; i -= HeaderSeed )
			{
				v17 = (i >> 2) & 3;
				for ( j = HeaderSizeInDWords - 1; j > 0; --j )
				{
					Header[j] -= ((Header[j - 1] ^ BBAHeader[v17 ^ j & 3]) + (v23 ^ i)) ^ ((16 * Header[j - 1] ^ (v23 >> 3))
						+ (4 * v23 ^ (Header[j - 1] >> 5)));
					v23 = Header[j];
				}
				*Header -= ((Header[HeaderSizeInDWords - 1] ^ BBAHeader[v17 ^ j & 3]) + (v23 ^ i)) ^ ((16
					* Header[HeaderSizeInDWords - 1] ^ (v23 >> 3))
					+ (4 * v23 ^ (Header[HeaderSizeInDWords - 1] >> 5)));
				v23 = *Header;
			}
			return true;
		}
	}
	
	return false;
}


bool __cdecl EncryptHeaderWithKey(PUINT Header, UINT HeaderSizeInBytes, PUINT Key, UINT KeySizeInBytes)
{
	char result;
	int v14, v19;
	unsigned int i, j;
	unsigned int v17;
	unsigned int NumberOfPasses = HeaderNumberOfPasses;
	unsigned int HeaderSizeInDWords;

	if ( Header && (result = HeaderSizeInBytes & 3, !(HeaderSizeInBytes & 3)) && Key && KeySizeInBytes >= 0x10 )
	{
		HeaderSizeInDWords = HeaderSizeInBytes >> 2;		
		if ( (signed int)(HeaderSizeInBytes >> 2) > 1 )
		{
			v17 = 0;
			v19 = NumberOfPasses;
			for ( i = Header[HeaderSizeInDWords - 1]; ; i = Header[HeaderSizeInDWords - 1] )
			{
				result = v19--;
				if ( result <= 0 )
					break;

				v17 += HeaderSeed;
				v14 = (v17 >> 2) & 3;
				for ( j = 0; j < (signed int)(HeaderSizeInDWords - 1); ++j )
				{
					Header[j] += ((i ^ Key[v14 ^ j & 3]) + (Header[j + 1] ^ v17)) ^ ((16 * i ^ (Header[j + 1] >> 3))
						+ (4 * Header[j + 1] ^ (i >> 5)));
					i = Header[j];
				}
				Header[HeaderSizeInDWords - 1] += ((i ^ Key[v14 ^ j & 3]) + (*Header ^ v17)) ^ ((16 * i ^ (*Header >> 3))
					+ (4 * *Header ^ (i >> 5)));
			}
			return true;
		}
	}

	return false;
}



bool __cdecl InitKey(PUINT TempKey, CONST PUINT HeaderKey, PUINT BBAHeader, UINT BBAHeaderSize)
{
	unsigned int i;

	if ( BBAHeader && BBAHeaderSize >= 4 )
	{
		for ( i = 0; i < 4; ++i )
		{
			TempKey[i] = BBAHeader[i] ^ HeaderKey[i];			
		}
		return true;
	}	
	return false;
}



bool __cdecl GetIndexTableKey(PUINT OutKey, PUINT IndexTableKey, PUINT Header, UINT HeaderSize)
{
	bool result; // al@6
	unsigned int i; // [sp+4h] [bp-4h]@3

	if ( Header && HeaderSize >= 4 )
	{
		for ( i = 0; i < 4; ++i )
			OutKey[i] = Header[i] ^ IndexTableKey[i];
		result = true;
	}
	else
	{
		result = false;
	}
	return result;
}

bool __cdecl DecryptIndexTableWithKey(PUINT IndexTable, UINT IndexTableSizeInBytes, PUINT Key, UINT KeySizeInBytes)
{
	int v17;
	int j;
	unsigned int i;	
	unsigned int v23, v22;
	unsigned int NumberOfPasses = TableNumberOfPasses;

	if ( IndexTable && !(IndexTableSizeInBytes & 3) && Key && KeySizeInBytes >= 0x10 )
	{
		v22 = IndexTableSizeInBytes >> 2;
		if ( (signed int)(IndexTableSizeInBytes >> 2) > 1 )
		{
			v23 = *IndexTable;			
			for ( i = TableSeed * NumberOfPasses; i; i -= TableSeed )
			{
				v17 = (i >> 2) & 3;
				for ( j = v22 - 1; j > 0; --j )
				{
					IndexTable[j] -= ((IndexTable[j - 1] ^ Key[v17 ^ j & 3]) + (v23 ^ i)) ^ ((16 * IndexTable[j - 1] ^ (v23 >> 3))
						+ (4 * v23 ^ (IndexTable[j - 1] >> 5)));
					v23 = IndexTable[j];
				}
				*IndexTable -= ((IndexTable[v22 - 1] ^ Key[v17 ^ j & 3]) + (v23 ^ i)) ^ ((16 * IndexTable[v22 - 1] ^ (v23 >> 3))
					+ (4 * v23 ^ (IndexTable[v22 - 1] >> 5)));
				v23 = *IndexTable;
			}
			return true;
		}
	}

	return false;
}

bool __cdecl EncryptIndexTableWithKey(PUINT IndexTable, UINT IndexTableSize, PUINT Key, UINT KeySizeInBytes)
{
	int result; // eax@2	
	int v14; // [sp+8h] [bp-24h]@12
	unsigned int i; // [sp+10h] [bp-1Ch]@10
	unsigned int v17; // [sp+14h] [bp-18h]@8
	UINT NumberOfPasses = TableNumberOfPasses;
	UINT KeySizeInDWords; // [sp+1Ch] [bp-10h]@6
	int j; // [sp+28h] [bp-4h]@12

	if ( IndexTable && (result = IndexTableSize & 3, !(IndexTableSize & 3)) && Key && KeySizeInBytes >= 0x10 )
	{
		KeySizeInDWords = IndexTableSize >> 2;
		if ( (signed int)(IndexTableSize >> 2) > 1 )
		{
			v17 = 0;			
			for ( i = IndexTable[KeySizeInDWords - 1]; ; i = IndexTable[KeySizeInDWords - 1] )
			{
				result = NumberOfPasses--;
				if ( result <= 0 )
					break;
				v17 += TableSeed;
				v14 = (v17 >> 2) & 3;
				for ( j = 0; j < (int)(KeySizeInDWords - 1); ++j )
				{
					IndexTable[j] += ((i ^ Key[v14 ^ j & 3]) + (IndexTable[j + 1] ^ v17)) ^ ((16 * i ^ (IndexTable[j + 1] >> 3))
						+ (4 * IndexTable[j + 1] ^ (i >> 5)));
					i = IndexTable[j];
				}
				IndexTable[KeySizeInDWords - 1] += ((i ^ Key[v14 ^ j & 3]) + (*IndexTable ^ v17)) ^ ((16 * i ^ (*IndexTable >> 3))
					+ (4 * *IndexTable ^ (i >> 5)));
			}
			return true;
		}
	}
	return false;
}




bool __stdcall EncryptHeader(PUINT Header, UINT HeaderSize, PUINT BBAHeader, UINT BBAHeaderSize)
{
	UINT TempKey[4];

	if( InitKey(TempKey, (CONST PUINT)HeaderKey, BBAHeader, BBAHeaderSize) )
	{
		return EncryptHeaderWithKey(Header, HeaderSize, TempKey, 0x10);
	}
	return false;
}

bool __stdcall DecryptHeader(PUINT Header, UINT HeaderSize, PUINT BBAHeader, UINT BBAHeaderSize)
{
	UINT TempKey[4];

	if( InitKey(TempKey, (CONST PUINT)HeaderKey, BBAHeader, BBAHeaderSize) )
	{
		return DecryptHeaderWithKey(Header, HeaderSize, TempKey, 0x10u);
	}

	return false;
}


bool __stdcall DecryptIndexTable(PUINT IndexTable, UINT IndexTableSize, PUINT Header, UINT HeaderSize)
{
	unsigned int TempHeader[4];

	if( HeaderSize == 0 )
	{
		memset( TempHeader, 0, 0x10 );
		HeaderSize  = 0x10;
	}
	else
	{
		memcpy( TempHeader, Header, 0x10 );
	}

	for( int i = 0; i < 4; i++ )
	{
		TempHeader[i] ^= TableKey[i];
	}

	unsigned int TempKey[4];

	if ( GetIndexTableKey(TempKey, (PUINT)IndexTableKey, TempHeader, 0x10) )
	{
		return DecryptIndexTableWithKey(IndexTable, IndexTableSize, TempKey, 0x10u);
	}
	return false;
}

bool __stdcall EncryptIndexTable(PUINT IndexTable, UINT IndexTableSize, PUINT Header, UINT HeaderSize)
{
	unsigned int TempHeader[4];

	if( HeaderSize == 0 )
	{
		memset( TempHeader, 0, 0x10 );
		HeaderSize  = 0x10;
	}
	else
	{
		memcpy( TempHeader, Header, 0x10 );
	}

	for( int i = 0; i < 4; i++ )
	{
		TempHeader[i] ^= TableKey[i];
	}

	unsigned int TempKey[4];

	if ( GetIndexTableKey(TempKey, (PUINT)IndexTableKey, TempHeader, 0x10) )
	{
		return EncryptIndexTableWithKey(IndexTable, IndexTableSize, TempKey, 0x10u);
	}
	return false;
}


bool __stdcall DecryptFileHeader(PUINT FileHeader, unsigned int FileHeaderSize, PUINT Header, UINT HeaderSize, bool UseTableKey )
{
	PUINT Key = (PUINT)HKey;

	if( UseTableKey )
	{
		Key = (PUINT)TableKey;
	}

	for( int i = 0; i < 4; i++ )
	{
		Header[i] ^= Key[i];
	}

	if ( FileHeaderSize >= 0x10 && Header && HeaderSize >= 0x10 )
	{
		if( IsDemoMode )
		{
			FileHeader[3] ^= Header[3] ^ FileHeaderKey[2] ^ FileHeader[2];
			FileHeader[2] ^= Header[2] ^ FileHeaderKey[1] ^ FileHeader[1];
			FileHeader[1] ^= Header[1] ^ FileHeaderKey[0] ^ FileHeader[0];
			FileHeader[0] ^= Header[0] ^ FileHeaderKey[3] ^ FileHeader[3];
		}
		else
		{
			FileHeader[3] ^= Header[3] ^ FileHeaderKey[3] ^ FileHeader[2];
			FileHeader[2] ^= Header[2] ^ FileHeaderKey[2] ^ FileHeader[1];
			FileHeader[1] ^= Header[1] ^ FileHeaderKey[1] ^ FileHeader[0];			
			FileHeader[0] ^= Header[0] ^ FileHeaderKey[0] ^ FileHeader[3];
		}
		return true;
	}
	return false;
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		SetMode( true );
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:		
		break;
	}
	return TRUE;
}

