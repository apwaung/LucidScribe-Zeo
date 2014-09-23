using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using ZeoScope;
using System.Drawing;

namespace lucidcode.LucidScribe.Plugin.Zeo
{
  public class RawEventArgs : EventArgs
  {
    public RawEventArgs(int current)
    {
      this.Current = current;
    }

    public int Current { get; private set; }
  }

  public static class Device
  {
    private static bool m_boolInitialized;
    private static bool m_boolInitError;
    private static bool disposed = false;

    static int eegLastPosition = 0;
    static int freqLastPosition = 0;
    static int stageLastPosition = 0;

    private static ZeoStream zeoStream;
    private static ManualResetEvent exitEvent = new ManualResetEvent(false);

    public static String ZeoPort = "";

    public static Boolean Arduino = false;
    public static String ArduinoPort = "COM1";
    public static String ArduinoDelay = "1";
    public static String ArduinoOn = "1";
    public static String ArduinoOff = "0";
    static Thread zeoThread;

    public static EventHandler<RawEventArgs> ZeoChanged;

    private static bool ClearDisplay;
    private static double DisplayValue;
    private static double StageValue;

    public static Boolean Initialize()
    {
      try
      {
        if (m_boolInitError) { return false; }

        if (!m_boolInitialized)
        {


          PortForm formPort = new PortForm();
          if (formPort.ShowDialog() == DialogResult.OK)
          {
            Arduino = formPort.Arduino;
            ArduinoPort = formPort.ArduinoPort;
            ArduinoDelay = formPort.ArduinoDelay;
            ArduinoOn = formPort.ArduinoOn;
            ArduinoOff = formPort.ArduinoOff;

            ZeoPort = formPort.SelectedPort;

            zeoStream = new ZeoStream(exitEvent);
            if (zeoStream.OpenLiveStream(ZeoPort))
            {
              zeoThread = new Thread(new ThreadStart(UpdateZeo));
              zeoThread.Start();
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

    private static void UpdateZeo()
    {
      do
      {
        ChannelData[] channels = zeoStream.ReadEegFromLastPosition(ref eegLastPosition, 128);
        if (channels.Length > 0)
        {
          double total = 0;
          foreach (ChannelData channel in channels)
          {
            int calibratedValue = Convert.ToInt32(((channel.Values[0]) * 10) + 3000) / 6;
            if (calibratedValue < 0) calibratedValue = 0;
            if (calibratedValue > 999) calibratedValue = 999;

            total += calibratedValue;

            if (ZeoChanged != null)
            {
              RawEventArgs e = new RawEventArgs(calibratedValue);
              ZeoChanged(null, e);
            }
          }
          DisplayValue = total / 128;
        }

        channels = zeoStream.ReadStageDataFromLastPosition(ref stageLastPosition, 1);
        if (channels.Length > 0)
        {
          StageValue = channels[0].Values[0] * -100;
        }
        if (disposed) { break; }

        Thread.Sleep(1000);
      } while (true);
    }

    public static void Dispose()
    {
      if (m_boolInitialized)
      {
        exitEvent.Set();
        disposed = true;
      }
    }

    public static Double GetValueEEG()
    {
      double temp = DisplayValue;
      ClearDisplay = true;
      return DisplayValue;
    }

    public static Double GetValueStage()
    {
      return StageValue;
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


  namespace RAW
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.ILluminatedPlugin
    {
      public string Name
      {
        get { return "Zeo RAW"; }
      }
      public bool Initialize()
      {
        bool initialized = Device.Initialize();
        Device.ZeoChanged += Device_ZeoChanged;
        return initialized;
      }

      public event Interface.SenseHandler Sensed;
      public void Device_ZeoChanged(object sender, RawEventArgs e)
      {
        if (ClearTicks)
        {
          ClearTicks = false;
          TickCount = "";
        }
        TickCount += e.Current + ",";

        if (ClearBuffer)
        {
          ClearBuffer = false;
          BufferData = "";
        }
        BufferData += e.Current + ",";
      }

      public void Dispose()
      {
        Device.ZeoChanged -= Device_ZeoChanged;
        Device.Dispose();
      }

      public Boolean isEnabled = false;
      public Boolean Enabled
      {
        get
        {
          return isEnabled;
        }
        set
        {
          isEnabled = value;
        }
      }

      public Color PluginColor = Color.White;
      public Color Color
      {
        get
        {
          return Color;
        }
        set
        {
          Color = value;
        }
      }

      private Boolean ClearTicks = false;
      public String TickCount = "";
      public String Ticks
      {
        get
        {
          ClearTicks = true;
          String ticks = TickCount;
          TickCount = "";
          return ticks;
        }
        set
        {
          TickCount = value;
        }
      }

      private Boolean ClearBuffer = false;
      public String BufferData = "";
      public String Buffer
      {
        get
        {
          ClearBuffer = true;
          String buffer = BufferData;
          BufferData = "";
          return buffer;
        }
        set
        {
          BufferData = value;
        }
      }

      int lastHour;
      public int LastHour
      {
        get
        {
          return lastHour;
        }
        set
        {
          lastHour = value;
        }
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
