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

    private static double StageValue;

    private static int[] frequencies = new int[8];
    private static int[] eigths = new int[8];
    private static List<int> tenths = new List<int>();

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

          if (channels.Length == 128)
          {
            for (int x = 0; x < 8; x++)
            {
              float maximum = 0;
              float minimum = 0;

              for (int y = 0; y < 16; y++)
              {
                float value = channels[(x * 16) + y].Values[0];
                if (value > maximum)
                {
                  maximum = value;
                }
                if (value < minimum)
                {
                  minimum = value;
                }
              }

              float greatest = maximum;
              if (minimum * -1 > maximum)
              {
                greatest = minimum;
              }

              eigths[x] = Convert.ToInt32((greatest * 10) + 3000) / 6;
            }
          }
        }

        channels = zeoStream.ReadFrequencyDataFromLastPosition(ref freqLastPosition, 1);
        if (channels.Length > 0 && channels[0] != null)
        {
          for (int i = 0; i < channels[0].Values.Length - 1; i++)
          {
            frequencies[i] = Convert.ToInt32((channels[0].Values[i] * 20));
          }
        }

        int stage = 0;
        channels = zeoStream.ReadStageDataFromLastPosition(ref stageLastPosition, 64, ref stage);
        StageValue = stage * -100;
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
      double eigth = DateTime.Now.Millisecond / 125;
      return eigths[(int)(Math.Round(eigth))];
    }

    public static Double GetValueDelta()
    {
      return frequencies[0];
    }

    public static Double GetValueTheta()
    {
      return frequencies[1];
    }

    public static Double GetValueAlpha()
    {
      return frequencies[2];
    }

    public static Double GetValueBeta1()
    {
      return frequencies[3];
    }

    public static Double GetValueBeta2()
    {
      return frequencies[4];
    }

    public static Double GetValueBeta3()
    {
      return frequencies[5];
    }

    public static Double GetValueGamma()
    {
      return frequencies[6];
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

  namespace Delta
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "D"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueDelta();
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

  namespace Theta
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "T"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueTheta();
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

  namespace Alpha
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "A"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueAlpha();
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

  namespace Beta1
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "B1"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueBeta1();
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

  namespace Beta2
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "B2"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueBeta2();
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

  namespace Beta3
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "B3"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueBeta3();
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

  namespace Gamma
  {
    public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
    {
      public override string Name
      {
        get { return "G"; }
      }
      public override bool Initialize()
      {
        return Device.Initialize();
      }
      public override double Value
      {
        get
        {
          double dblValue = Device.GetValueGamma();
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
}
