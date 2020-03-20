// BuildIn.cpp: 定义 DLL 的初始化例程。
//

#include "pch.h"
#include "framework.h"
#include "BuildIn.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
#include "exports.h"

//
//TODO:  如果此 DLL 相对于 MFC DLL 是动态链接的，
//		则从此 DLL 导出的任何调入
//		MFC 的函数必须将 AFX_MANAGE_STATE 宏添加到
//		该函数的最前面。
//
//		例如: 
//
//		extern "C" BOOL PASCAL EXPORT ExportedFunction()
//		{
//			AFX_MANAGE_STATE(AfxGetStaticModuleState());
//			// 此处为普通函数体
//		}
//
//		此宏先于任何 MFC 调用
//		出现在每个函数中十分重要。  这意味着
//		它必须作为以下项中的第一个语句:
//		出现，甚至先于所有对象变量声明，
//		这是因为它们的构造函数可能生成 MFC
//		DLL 调用。
//
//		有关其他详细信息，
//		请参阅 MFC 技术说明 33 和 58。
//

// CBuildInApp

BEGIN_MESSAGE_MAP(CBuildInApp, CWinApp)
END_MESSAGE_MAP()
HWND targetWnd = NULL;
CWnd* pDesktop = NULL;
CDC* pdeskdc = NULL;

HMODULE hMse = NULL;

POINT p1;
POINT p2;

bool ncase = true;

HHOOK hOOk = NULL;

RECT* result = NULL;

LRESULT CALLBACK MouseProc(int code ,WPARAM wParam,LPARAM lParam) {
    if (wParam == WM_LBUTTONDOWN)
        return TRUE;
    if(wParam!=WM_LBUTTONUP)
        return CallNextHookEx(hOOk, code, wParam, lParam);


    if (ncase) {
        GetCursorPos(&p1);
        ScreenToClient(targetWnd, &p1);
        ncase = false;
        return TRUE;
    }else
    if (!ncase) {
        GetCursorPos(&p2);
        ScreenToClient(targetWnd, &p2);

        UnhookWindowsHookEx(hOOk);
        hOOk = NULL;

    //    AfxMessageBox(L"Completed");
        result->top = p1.y<0?0: p1.y;
        result->left = p1.x < 0 ? 0 : p1.x;
        result->right = p2.x<0?100:p2.x;
        result->bottom = p2.y<0?100:p2.y;
        compis = true;
        return TRUE;
    }

    return CallNextHookEx(hOOk, code, wParam, lParam);
}



RECT*getRate(HWND hWnd) {
    targetWnd = hWnd;
    pDesktop = CWnd::FromHandle(targetWnd);
    pdeskdc = pDesktop->GetDC();

    AfxMessageBox(L"接下来请在目标窗口上点击两次。\n程序会用这两个点构造一个矩形，这就是放大区域");
    typedef    void    (WINAPI* PROCSWITCHTOTHISWINDOW)    (HWND, BOOL);
    PROCSWITCHTOTHISWINDOW    SwitchToThisWindow;
    HMODULE    hUser32 = GetModuleHandle(L"user32");
    SwitchToThisWindow = (PROCSWITCHTOTHISWINDOW)
        GetProcAddress(hUser32, "SwitchToThisWindow");

    SwitchToThisWindow(hWnd, TRUE);
    hOOk = SetWindowsHookEx(WH_MOUSE_LL, MouseProc, hMse, 0);
   // while (hOOk)
   //     Sleep(10);


   result=new RECT;


    
   

   

    return result;
}

PBYTE LastMemaddr = NULL;

PBYTE ScreenShot(int* nbsize)
{
    CBitmap bmp;
    CRect re;
    BITMAP bit;

    //获取窗口的大小 
    pDesktop->GetClientRect(&re);
 
    bmp.CreateCompatibleBitmap(pdeskdc, re.Width(), re.Height());
    //创建一个兼容的内存画板 
    CDC memorydc;
    memorydc.CreateCompatibleDC(pdeskdc);
    //选中画笔 
    memorydc.SelectObject(&bmp);
    //绘制图像 
    memorydc.BitBlt(0, 0, re.Width(), re.Height(), pdeskdc, 0, 0, SRCCOPY);

    bmp.GetBitmap(&bit);
    //定义 图像大小（单位：byte） 
    DWORD size = bit.bmWidthBytes * bit.bmHeight;
    LPSTR lpdata = (LPSTR)GlobalAlloc(GPTR, size);
    //后面是创建一个bmp文件的必须文件头 
    BITMAPINFOHEADER pbitinfo;
    pbitinfo.biBitCount = 24;
    pbitinfo.biClrImportant = 0;
    pbitinfo.biCompression = BI_RGB;
    pbitinfo.biHeight = bit.bmHeight;
    pbitinfo.biPlanes = 1;
    pbitinfo.biSize = sizeof(BITMAPINFOHEADER);
    pbitinfo.biSizeImage = size;
    pbitinfo.biWidth = bit.bmWidth;
    pbitinfo.biXPelsPerMeter = 0;
    pbitinfo.biYPelsPerMeter = 0;

    GetDIBits(pdeskdc->m_hDC, bmp, 0, pbitinfo.biHeight, lpdata, (BITMAPINFO*)
        &pbitinfo, DIB_RGB_COLORS);

    BITMAPFILEHEADER bfh;
    bfh.bfReserved1 = bfh.bfReserved2 = 0;
    bfh.bfType = ((WORD)('M' << 8) | 'B');
    bfh.bfSize = size + 54;
    bfh.bfOffBits = 54;

    //构建内存
    PBYTE bpes = new BYTE[sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER) + size];

    memcpy_s(bpes, sizeof(BITMAPFILEHEADER), &bfh, sizeof(BITMAPFILEHEADER));
    memcpy_s(bpes+ sizeof(BITMAPFILEHEADER), sizeof(BITMAPINFOHEADER), &pbitinfo, sizeof(BITMAPINFOHEADER));
    memcpy_s(bpes + sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER), size, lpdata, size);

    LastMemaddr = bpes;
    GlobalFree(lpdata);
    *nbsize = size;

    return bpes;
}

void FreeMemory(void) {
    if (LastMemaddr)
        delete[]LastMemaddr;
}

// CBuildInApp 构造

CBuildInApp::CBuildInApp()
{
	// TODO:  在此处添加构造代码，
	// 将所有重要的初始化放置在 InitInstance 中
}


// 唯一的 CBuildInApp 对象

CBuildInApp theApp;


// CBuildInApp 初始化

BOOL CBuildInApp::InitInstance()
{
	CWinApp::InitInstance();
//	AfxMessageBox(L"AVB");
    hMse = this->m_hInstance;
	return TRUE;
}


int CBuildInApp::ExitInstance()
{
	// TODO: 在此添加专用代码和/或调用基类

	return CWinApp::ExitInstance();
}
