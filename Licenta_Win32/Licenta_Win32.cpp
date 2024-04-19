// Licenta_Win32.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#define _WINSOCK_DEPRECATED_NO_WARNINGS

#ifndef _WINDOWS_
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#undef WIN32_LEAN_AND_MEAN
#endif

#include <winsock2.h>

#pragma comment(lib, "ws2_32.lib")

#include <iostream>
using namespace std;

POINTER_TYPE_INFO pointerTypeInfo;
POINTER_PEN_INFO penInfo;
POINTER_INFO pointerInfo;
POINT coords;

const int screenWidth = GetSystemMetrics(SM_CXSCREEN);
const int screenHeight = GetSystemMetrics(SM_CYSCREEN);
HSYNTHETICPOINTERDEVICE synthPointer;

uint8_t i2cPosData[5];
uint8_t i2cPressData[2];

typedef union {
    struct {
        UINT16 X;
        UINT16 Y;
        UINT16 Force;
        UINT16 Flags;
    };
    UINT64 Data;
} dataPack;

const char* ip = "0.0.0.0";
const int port = 44441;

void updatePen(int x, int y, int pressure) {
    const int maxPressure = 1 << 12;
    const int pressureOffset = 1600;
    int calcPressure = (pressure - pressureOffset) * 1024 / maxPressure;
    if (calcPressure > 1023) pressure = 1023;
    if (calcPressure < 0) pressure = 0; 
    coords.x = x;
    coords.y = y;
    pointerInfo.ptPixelLocation = coords;
    penInfo.pointerInfo = pointerInfo;
    penInfo.pressure = calcPressure;
    pointerTypeInfo.penInfo = penInfo; //some stuff might be redundant
}

void updatePointerFlag(int pointerFlags) {
    pointerInfo.pointerFlags = pointerFlags;
    penInfo.pointerInfo = pointerInfo;
    pointerTypeInfo.penInfo = penInfo; //some stuff may be redundant
}

int main() {   
    synthPointer = CreateSyntheticPointerDevice(PT_PEN, 1, POINTER_FEEDBACK_INDIRECT);

    pointerInfo.pointerType = PT_PEN;
    pointerInfo.pointerId = 0;
    pointerInfo.pointerFlags = POINTER_FLAG_INRANGE | POINTER_FLAG_PRIMARY;
    pointerInfo.ptPixelLocation = coords;

    penInfo.penFlags = PEN_FLAG_NONE;
    penInfo.penMask = PEN_MASK_PRESSURE;
    penInfo.pressure = 512;
    penInfo.pointerInfo = pointerInfo;

    pointerTypeInfo.penInfo = penInfo;
    pointerTypeInfo.type = PT_PEN;


    /*Socket server stuff*/

      //1. Initialize WSA variables
    WSADATA wsaData;
    int wsaerr;
    WORD wVersionRequested = MAKEWORD(2, 2);
    wsaerr = WSAStartup(wVersionRequested, &wsaData);
    //WSAStartup resturns 0 if it is successfull or non zero if failed
    if (wsaerr != 0) {
        cout << "The Winsock dll not found!" << endl;
        return 0;
    }
    else {
        cout << "The Winsock dll found" << endl;
        cout << "The status: " << wsaData.szSystemStatus << endl;
    }

    /*
    refer ServerCreation.md
    */
    SOCKET serverSocket;
    serverSocket = INVALID_SOCKET; //initializing as a inivalid socket
    serverSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    //check if creating socket is successfull or not
    if (serverSocket == INVALID_SOCKET) {
        cout << "Error at socket():" << WSAGetLastError() << endl;
        WSACleanup();
        return 0;
    }
    else {
        cout << "socket is OK!" << endl;
    }

    /*
    3. Bind the socket to ip address and port number
    //refer ServerCreation.md
    */
    sockaddr_in service; //initialising service as sockaddr_in structure
    service.sin_family = AF_INET;
    //InetPton(AF_INET, _T("127.0.0.1"), &service.sin_addr.s_addr); 
    //InetPton function is encountering an issue, so replaced with the following line which uses inet_addr to convert IP address string to the binary form (only for ipv4) and storing it
    service.sin_addr.s_addr = inet_addr(ip);
    //    service.sin_addr.s_addr = inet_addr("192.168.43.42");
    service.sin_port = htons(port);
    //using the bind function
    if (bind(serverSocket, (SOCKADDR*)&service, sizeof(service)) == SOCKET_ERROR) {
        cout << "bind() failed: " << WSAGetLastError() << endl;
        closesocket(serverSocket);
        WSACleanup();
        return 0;
    }
    else {
        cout << "bind() is OK!" << endl;
    }

    //4. Listen to incomming connections
    if (listen(serverSocket, 1) == SOCKET_ERROR) {
        cout << "listen(): Error listening on socket: " << WSAGetLastError() << endl;
    }
    else {
        cout << "listen() is OK!, I'm waiting for new connections..." << endl;
    }

    //5. accepting incomming connections
    SOCKET acceptSocket;
    acceptSocket = accept(serverSocket, NULL, NULL);
    if (acceptSocket == INVALID_SOCKET) {
        cout << "accept failed: " << WSAGetLastError() << endl;
        WSACleanup();
        return -1;
    }
    else {
        cout << "accept() is OK!" << endl;
    }


    while (true) {
        char receiveBuffer[200];
        int rbyteCount = recv(acceptSocket, receiveBuffer, 50, 0);
        if (rbyteCount < 0) {
            cout << "Server recv error: " << WSAGetLastError() << endl;
            return 0;
        }
        else {
            dataPack data;
            memcpy(&data.Data, receiveBuffer, 8);
            cout << data.X << '\t' << data.Y << '\t' << data.Force << endl;

            if (data.Flags = 1) {
                updatePen(data.X, data.Y, data.Force);
                updatePointerFlag(POINTER_FLAG_INRANGE);
                InjectSyntheticPointerInput(synthPointer, &pointerTypeInfo, 1);
            }
            else {
                updatePointerFlag(POINTER_FLAG_CANCELED);
            }
        }
    }


    return 0;
}

