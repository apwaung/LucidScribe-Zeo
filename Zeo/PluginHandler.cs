using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using ZeoScope;

namespace lucidcode.LucidScribe.Plugin.Zeo
{

  public static class Device
  {
    private static bool m_boolInitialized;
    private static bool m_boolInitError;

    static int eegLastPosition = 0;
    static int stageLastPosition = 0;

    private static ZeoStream zeoStream;
    private static ManualResetEvent exitEvent = new ManualResetEvent(false);

    public static Boolean Arduino = false;
    public static String ArduinoPort = "COM1";
    public static String ArduinoDelay = "1";
    public static String ArduinoOn = "1";
    public static String ArduinoOff = "0";

    public static Boolean Initialize()
    {
      try
      {
        if (m_boolInitError) { return false; }

        if (!m_boolInitialized)
        {
          zeoStream = new ZeoStream(exitEvent);

          PortForm formPort = new PortForm();
          if (formPort.ShowDialog() == DialogResult.OK)
          {
            Arduino = formPort.Arduino;
            ArduinoPort = formPort.ArduinoPort;
            ArduinoDelay = formPort.ArduinoDelay;
            ArduinoOn = formPort.ArduinoOn;
            ArduinoOff = formPort.ArduinoOff;

            if (zeoStream.OpenLiveStream(formPort.SelectedPort))
            {
              m_boolInitialized = true;
              return true;
            }
          }

          m_boolInitError = true;
          return false;
        }
        return true;
      }
      catch (Exception ex)
      {
        m_boolInitError = true;
        throw (new Exception("The 'Zeo' plugin failed to initialize: " + ex.Message));
      }
    }

    public static void Dispose()
    {
      if (m_boolInitialized)
      {
        exitEvent.Set();
      }
    }

    public static Double GetValueEEG()
    {
      ChannelData[] channels = zeoStream.ReadEegFromLastPosition(ref eegLastPosition, 1);
      if (channels.Length > 0)
      {
        return channels[0].Values[0] * 1000;
      }
      return 0;
    }

    public static Double GetValueStage()
    {
      ChannelData[] channels = zeoStream.ReadStageDataFromLastPosition(ref stageLastPosition, 1);
      if (channels.Length > 0)
      {
        return channels[0].Values[0] * -100;
      }
      return 0;
    }

  }

  namespace ZeoEEG
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "Zeo EEG"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueEEG();
          if (dblValue > 999) { dblValue = 999; }
          if (dblValue < 0) { dblValue = 0; }
          return dblValue;
        }
      }
      public override void Dispose()
      {
        Device.Dispose();
      }
    }
  }

  namespace Stage
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      Thread ArduinoThread;
      public override string Name
      {
        get { return "Stage"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueStage();

          // Check if we are dreaming
          if (dblValue == 200)
          {
            // Check if we need to send a message to an arduino
            if (Device.Arduino)
            {
              Device.Arduino = false; // Set false so we don't call it again before the thread completes / after the delay
              ArduinoThread = new Thread(TriggerArduino);
              ArduinoThread.Start();
            }
          }

          if (dblValue > 999) { dblValue = 999; }
          if (dblValue < 0) { dblValue = 0; }
          return dblValue;
        }
      }

      private void TriggerArduino()
      {
        SerialPort arduinoPort = new SerialPort();
        arduinoPort.PortName = Device.ArduinoPort;
        arduinoPort.BaudRate = 9600;
        arduinoPort.Open();

        arduinoPort.WriteLine(Device.ArduinoOn);

        int arduinoDelay = Convert.ToInt32(Device.ArduinoDelay) * 60000;
        Thread.Sleep(arduinoDelay);

        arduinoPort.WriteLine(Device.ArduinoOff);

        arduinoPort.Close();
        arduinoPort.Dispose();

        Device.Arduino = true;
      }
    }
  }
}
