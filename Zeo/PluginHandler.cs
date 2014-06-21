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
      float total = 0;

      ChannelData[] channels = zeoStream.ReadEegFromLastPosition(ref eegLastPosition, 100);

      foreach (ChannelData channelData in channels)
      {
        foreach (float value in channelData.Values)
        {
          total += value;
        }
      }

      return total;
    }

    public static Double GetValueStage()
    {
      ChannelData[] channels = zeoStream.ReadStageDataFromLastPosition(ref stageLastPosition, 100);
      if (channels.Length > 0)
      {
        return channels[0].Values[0] * 100;
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
          if (dblValue > 999) { dblValue = 999; }
          if (dblValue < 0) { dblValue = 0; }
          return dblValue;
        }
      }
    }
  }
}
