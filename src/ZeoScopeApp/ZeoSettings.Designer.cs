﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeoScope {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class ZeoSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ZeoSettings defaultInstance = ((ZeoSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ZeoSettings())));
        
        public static ZeoSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LucidAlarmEnabled {
            get {
                return ((bool)(this["LucidAlarmEnabled"]));
            }
            set {
                this["LucidAlarmEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ComPortSelectedIndex {
            get {
                return ((int)(this["ComPortSelectedIndex"]));
            }
            set {
                this["ComPortSelectedIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string MP3FileName {
            get {
                return ((string)(this["MP3FileName"]));
            }
            set {
                this["MP3FileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LucidAlarmCue {
            get {
                return ((string)(this["LucidAlarmCue"]));
            }
            set {
                this["LucidAlarmCue"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LucidAlarmFromTime {
            get {
                return ((string)(this["LucidAlarmFromTime"]));
            }
            set {
                this["LucidAlarmFromTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LucidAlarmToTime {
            get {
                return ((string)(this["LucidAlarmToTime"]));
            }
            set {
                this["LucidAlarmToTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int EegLevelSelecteIndex {
            get {
                return ((int)(this["EegLevelSelecteIndex"]));
            }
            set {
                this["EegLevelSelecteIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ZeoFileNames {
            get {
                return ((string)(this["ZeoFileNames"]));
            }
            set {
                this["ZeoFileNames"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool WindowMaximized {
            get {
                return ((bool)(this["WindowMaximized"]));
            }
            set {
                this["WindowMaximized"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("600")]
        public int WindowHeight {
            get {
                return ((int)(this["WindowHeight"]));
            }
            set {
                this["WindowHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("800")]
        public int WindowWidth {
            get {
                return ((int)(this["WindowWidth"]));
            }
            set {
                this["WindowWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int FreqPanelHeight {
            get {
                return ((int)(this["FreqPanelHeight"]));
            }
            set {
                this["FreqPanelHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public int FadeIn {
            get {
                return ((int)(this["FadeIn"]));
            }
            set {
                this["FadeIn"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int FadeOut {
            get {
                return ((int)(this["FadeOut"]));
            }
            set {
                this["FadeOut"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("120")]
        public int AlarmDuration {
            get {
                return ((int)(this["AlarmDuration"]));
            }
            set {
                this["AlarmDuration"] = value;
            }
        }
    }
}