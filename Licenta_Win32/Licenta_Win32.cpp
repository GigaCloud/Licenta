// Licenta_Win32.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <windows.h>
#include <mcp2221_dll_um.h>


POINTER_TYPE_INFO pointerTypeInfo;
POINTER_PEN_INFO penInfo;
POINTER_INFO pointerInfo;
POINT coords;

void* MCPHandle = NULL;
const int VID = 0x4D8;
const int PID = 0xDD; //default MCP2221 vals
const int I2CSpeed = 100000; //seems stable at this speed
const byte AR1000Address = 0x4D; //7 bit address
const int NO_POS_BYTES = 5; //AR1000 datasheet, 5 bytes for pos

const int NO_PRESS_BYTES = 2;
const byte FMAAddress = 0x28; //7 bit, Honeywell pressure sensor FMAMSDXX005WC2C3

const int xMax = 1 << 12;
const int yMax = 1 << 12;
const float edgePercent = 12.5f;

const int screenWidth = GetSystemMetrics(SM_CXSCREEN);
const int screenHeight = GetSystemMetrics(SM_CYSCREEN);


void updatePen(int x, int y, int pressure) {
    const int maxPressure = 1 << 12;
    const int pressureOffset = 1000;
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
    pointerInfo.pointerFlags = POINTER_FLAG_INCONTACT | POINTER_FLAG_PRIMARY;
    penInfo.pointerInfo = pointerInfo;
    pointerTypeInfo.penInfo = penInfo; //some stuff may be redundant
}


int initMCP(void) {
    int err = 0;

    MCPHandle = Mcp2221_OpenByIndex(VID, PID, 0);

    if (reinterpret_cast<std::size_t>(MCPHandle) == -1) {
        std::cout << "Could not connect to MCP!\n";
        return -1;
    }
    else {
        std::cout << "MCP Connected!\n";
    }

    err = Mcp2221_SetSpeed(MCPHandle, I2CSpeed);

    if (err != 0) {
        std::cout << "Could not set bus speed! Trying to close the current handle...\n";
        Mcp2221_CloseAll();
        MCPHandle = Mcp2221_OpenByIndex(VID, PID, 0);

        if (reinterpret_cast<std::size_t>(MCPHandle) == -1) {
            std::cout << "Could not reconnect to MCP!\n";
            return -1;
        }
        else {
            std::cout << "Reconnected succsefully; Cancelling current transfer and retrying speed set...\n";
            Mcp2221_I2cCancelCurrentTransfer(MCPHandle);
            err = Mcp2221_SetSpeed(MCPHandle, I2CSpeed);
            if (err != 0) {
                std::cout << "Could not set bus speed! Error: " << err << " Exiting...\n";
                return -1;
            }
            else {
                std::cout << "Bus reconnected succsefully! :D \n";
            }
        }
    }
    return 0;
}

int main() {   
    byte i2cPosData[5];
    byte i2cPressData[2];

    HSYNTHETICPOINTERDEVICE synthPointer = CreateSyntheticPointerDevice(PT_PEN, 1, POINTER_FEEDBACK_DEFAULT);

    pointerInfo.pointerType = PT_PEN;
    pointerInfo.pointerId = 0;
    pointerInfo.pointerFlags = POINTER_FLAG_INCONTACT | POINTER_FLAG_PRIMARY;
    pointerInfo.ptPixelLocation = coords;

    penInfo.penFlags = PEN_FLAG_NONE;
    penInfo.penMask = PEN_MASK_PRESSURE;
    penInfo.pressure = 512;
    penInfo.pointerInfo = pointerInfo;

    pointerTypeInfo.penInfo = penInfo;
    pointerTypeInfo.type = PT_PEN;

    if (initMCP() != 0) return -1; //Critical failure, could not connect to MCP2221


    while (true) {

        int err = Mcp2221_I2cRead(MCPHandle, NO_POS_BYTES, AR1000Address, 1, i2cPosData);
        err = Mcp2221_I2cRead(MCPHandle, NO_PRESS_BYTES, FMAAddress, 1, i2cPressData);

        int y = (int)(i2cPosData[1] | (i2cPosData[2] << 7)); //refer to the AR1000 doc, table 7.1
        int x = (int)(i2cPosData[3] | (i2cPosData[4] << 7)); //warning, x/y are swapped here!

        int force = (int)((i2cPressData[0] << 7) | i2cPressData[1]);


        double xr = 1.0f - (x / (double)xMax); //because I want it mirrored
        double yr = y / (double)yMax;
        
        if(x != 0 && y != 0){
            std::cout << xr << '\t' << yr << '\n';
            int xCursor = (int)((xr * screenWidth * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenWidth / 100.0f));
            int yCursor = (int)((yr * screenHeight * (100 + edgePercent) / 100.0f - edgePercent * 0.5f * screenHeight / 100.0f));

            updatePen(xCursor, yCursor, force);
            updatePointerFlag(POINTER_FLAG_INRANGE | POINTER_FLAG_PRIMARY);
            InjectSyntheticPointerInput(synthPointer, &pointerTypeInfo, 1);
        }
        else {
            updatePointerFlag(POINTER_FLAG_UP);
        }

        Sleep(1);
    }
    return 0;
}

