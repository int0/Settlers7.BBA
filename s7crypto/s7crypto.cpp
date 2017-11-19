// s7crypto.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "s7crypto.h"


// This is an example of an exported variable
S7CRYPTO_API int ns7crypto=0;

// This is an example of an exported function.
S7CRYPTO_API int fns7crypto(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see s7crypto.h for the class definition
Cs7crypto::Cs7crypto()
{
	return;
}
