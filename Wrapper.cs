﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboticArm
{
    public static class Wrapper
    {
        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxStartSimulation(int clientID, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxStopSimulation(int clientID, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)] //[DllImport("remoteApi.dll")]
        public static extern void simxFinish(int clientID);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int simxGetConnectionId(int clientID);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetFloatSignal(int clientID, string signalName, ref float value, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetIntegerSignal(int clientID, string signalName, ref int value, simx_opmode opmode);

        public static int simwGetIntegerSignal(int clientID, string signalName)
        {
            int v = -1;
            simxGetIntegerSignal(clientID, signalName, ref v, simx_opmode.streaming);
            Thread.Sleep(150);
            simxGetIntegerSignal(clientID, signalName, ref v, simx_opmode.buffer);
            return v;
        }

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSetStringSignal(int clientID, string signalName, string value, int length, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetStringSignal(int clientID, string signalName, ref IntPtr pointerToValue, ref int signalLength, simx_opmode opmode);

        public static string simwGetStringSignal(int clientID, string signalName)
        {
            IntPtr p = IntPtr.Zero;
            var l1 = 0;
            var l2 = 0;
            var e = simxGetStringSignal(clientID, signalName, ref p, ref l1, simx_opmode.streaming);
            Thread.Sleep(150);
            e = simxGetStringSignal(clientID, signalName, ref p, ref l2, simx_opmode.buffer);
            Console.WriteLine("Signal {0} -> {1}/{2}", signalName, l1, l2);
            if (e == simx_error.noerror && p != IntPtr.Zero)
            {
                var s = Marshal.PtrToStringAnsi(p, l2);
                Marshal.Release(p);
                return s;
            }
            return "";
        }

      //  [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
    //    public static extern simx_error simxGetAndClearStringSignal(int clientID, string signalName, ref IntPtr pointerToValue, ref int signalLength, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetJointPosition(int clientID, int jointHandle, ref float targetPosition, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetObjectIntParameter(int clientID, int objectHandle, int parameterID, ref int parameterValue, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetObjectFloatParameter(int clientID, int objectHandle, int parameterID, ref float parameterValue, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetObjectOrientation(int clientID, int jointHandle, int relativeToHandle, float[] orientations, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetObjectPosition(int clientID, int jointHandle, int relativeToHandle, float[] positions, simx_opmode opmode);

      //  [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
    //    public static extern simx_error simxPauseCommunication(int cliendID, int pause);

      //  [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
      //  public extern static simx_error simxReadProximitySensor(int clientID, int sensorHandle,
               //                                          ref char detectionState, float[] detectionPoint, ref int objectHandle, float[] normalVector, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSetJointTargetPosition(int clientID, int jointHandle, float targetPosition, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSetJointTargetVelocity(int clientID, int jointHandle, float velocity, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSetObjectFloatParameter(int clientID, int objectHandle, int parameterID, float parameterValue, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSetObjectIntParameter(int clientID, int objectHandle, int parameterID, int parameterValue, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int simxStart(string ip, int port, bool waitForConnection, bool reconnectOnDisconnect, int timeoutMS, int cycleTimeMS);

      //  [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
      //  public static extern simx_error simxGetUIEventButton(int clientID, int uiHandle, ref int uiEventButtonID, IntPtr aux, simx_opmode opmode);

       // [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern simx_error simxGetUIHandle(int clientID, string uiName, out int handle, simx_opmode opmode);
     //   public static extern simx_error simxGetUIHandle(int clientID, string uiName, IntPtr p, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxGetObjectHandle(int clientID, string objectName, out int handle, simx_opmode opmode);       

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSynchronousTrigger(int clientID);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern simx_error simxSynchronous(int clientID, bool enable);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static simx_error simxGetVisionSensorImage(int clientID, int sensorHandle, out int resolution, out IntPtr image, char option, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static simx_error simxSetVisionSensorImage(int clientID, int sensorHandle, out IntPtr image, int bufferSize, char options, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static simx_error simxSetFloatSignal(int clientID, string signalName, float signalValue, simx_opmode opmode);

     //   [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
    //    public extern static simx_error simxReadProximitySensor(int clientID, int sensorHandle, out char detectionState, out float detectedPoint, out int detectedObjectHandle, out float detectedSurfaceNormalVector, simx_opmode opmode);

      //  [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
    //    public extern static simx_error simxReadVisionSensor(int clientID, int sensorHandle, out char detectionState, out float auxValues, out int auxValuesCount, simx_opmode opmode);

        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static simx_error simxSetIntegerSignal(int clientID, string signalName, int signalValue, simx_opmode opmode);



        //call function
        [DllImport("remoteApi.dll", CallingConvention = CallingConvention.Cdecl)]
        /*simxInt simxCallScriptFunction(const simxInt* inInt,simxInt inFloatCnt,const simxFloat* inFloat,simxInt inStringCnt,
                const simxChar* inString,simxInt inBufferSize,const simxUChar* inBuffer,simxInt* outIntCnt,simxInt** outInt,
                    simxInt* outFloatCnt,simxFloat** outFloat,simxInt* outStringCnt,simxChar** outString,simxInt* outBufferSize,
                simxUChar** outBuffer,simxInt operationMode);*/
        public static extern simx_error simxCallScriptFunction(int clientID, string scripDescription, sim_scripttype type, string functionName,
             int InIntCont, int[] InIntVal, int InFloatCnt, float[] InFloatValue, int InStringCnt, string InStringVAL, int BuffSize, byte buffer,
             out int OutInCnt, ref int[] OutIntVal, out int OutFloatCnt, ref float[] FloatVal, out int StringCnt, ref string[] OutStringValue
             , out int OutBufferSize, ref byte[] BufferValue, simx_opmode mode);
    }
}