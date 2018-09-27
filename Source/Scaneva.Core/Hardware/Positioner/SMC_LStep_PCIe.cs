﻿#region Copyright (C)
// ---------------------------------------------------------------------------------------------------------------
//  <copyright file="SMC_LStep_PCIe.cs" company="Scaneva">
// 
//  Copyright (C) 2018 Roche Diabetes Care GmbH (Christoph Pieper)
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <summary>
//  Url: https://github.com/Scaneva
//  </summary>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Scaneva.Core;
using Scaneva.Core.Settings;
using System.Runtime.InteropServices;
using System.IO;

using Scaneva.Tools;

namespace Scaneva.Core.Hardware
{
    [DisplayName("LANG LStep PCIe controller")]
    [Category("Positioner")]
    class SMC_LStep_PCIe : ParametrizableObject, IHWManager, IPositioner, ITransducer
    {
        private CClassLStep.LStep Positioner;
        private enuHWStatus mHWStatus = enuHWStatus.NotInitialized;
        private enuPositionerStatus mPosStatus = enuPositionerStatus.NotInitialized;

        public List<TransducerChannel> channels = new List<TransducerChannel>();
        Dictionary<long, String> ErrorList = new Dictionary<long, string>();

        public SMC_LStep_PCIe(LogHelper log) : base(log)
        {
            settings = new SMC_LStep_PCIe_Settings();
            log.Add("Initializing LANG LStep PCIe controller");
            InitTransducerChannels();

            ErrorList.Add(0, "Alles OK");
            ErrorList.Add(1, "Angegebene Achse nicht vorhanden");
            ErrorList.Add(2, "Funktion kann nicht ausgeführt werden");
            ErrorList.Add(3, "Zu viele Zeichen im Befehlsstring");
            ErrorList.Add(4, "Unbekannter Befehl");
            ErrorList.Add(5, "Außerhalb des gültigen Zahlenbereichs");
            ErrorList.Add(6, "Anzahl Parameter falsch");
            ErrorList.Add(7, "Befehl muss mit !oder ? beginnen");
            ErrorList.Add(8, "Kein TVR möglich, da Achse aktiv");
            ErrorList.Add(9, "Kein Ein - oder Ausschalten der Achsen, da TVR aktiv");
            ErrorList.Add(10, "Funktion nicht konfiguriert");
            ErrorList.Add(11, "Kein Move - Befehl möglich, da Joystick aktiv");
            ErrorList.Add(12, "Endschalter betätigt");
            ErrorList.Add(13, "Funktion kann nicht ausgeführt werden, da Encoder erkannt");
            ErrorList.Add(14, "Fehler beim Kalibrieren!Endschalter nicht korrekt freigefahren");
            ErrorList.Add(15, "Fehler beim Kalibrieren auf Referenzmarke");
            ErrorList.Add(16, "Save - Befehl fehlgeschlagen");
            ErrorList.Add(17, "Achse noch in Benutzung");
            ErrorList.Add(18, "Achse nicht bereit");
            ErrorList.Add(19, "Achse nicht kalibriert");
            ErrorList.Add(20, "Treiberrelais defekt(Sicherheitskreis K3 / K4)");
            ErrorList.Add(21, "Es dürfen nur einzelne Vektoren verfahren werden(Einrichtbetrieb)");
            ErrorList.Add(22, "Es darf kein Kalibrieren, Tischhubmessen oder Joystickbetrieb durchgeführt werden.(Tür offen oder Einrichtbetrieb)");
            ErrorList.Add(23, "SECURITY Error X - Achse");
            ErrorList.Add(24, "SECURITY Error Y - Achse");
            ErrorList.Add(25, "SECURITY Error Z - Achse");
            ErrorList.Add(26, "SECURITY Error A - Achse");
            ErrorList.Add(27, "Not - STOP");
            ErrorList.Add(28, "Fehler im Türschaltersicherheitskreis");
            ErrorList.Add(29, "Endstufen nicht eingeschaltet");
            ErrorList.Add(30, "GAL Sicherheitsfehler");
            ErrorList.Add(31, "Joystick lässt sich nicht einschalten, da move aktiv");
            ErrorList.Add(32, "Vektor außerhalb des Verfahrbereiches");

            ErrorList.Add(1010, "Anderer manueller Modus bereits aktiv");
            ErrorList.Add(1011, "Servo - und Schrittmotor nicht koppelbar(Joystick)");
            ErrorList.Add(1012, "Ausgang bereits anderer Funktion zugeordnet(digitaler Ausgang)");
            ErrorList.Add(1030, "Konfigurierung ist aktiv");

            ErrorList.Add(1031, "Achse nicht konfiguriert");
            ErrorList.Add(1032, "Interner Fehler");
            ErrorList.Add(1033, "Achse noch in Benutzung");
            ErrorList.Add(1034, "Achse in Fehlerstatus");
            ErrorList.Add(1035, "Achse nicht kalibriert");
            ErrorList.Add(1036, "Achse ohne RoomMeasure");
            ErrorList.Add(1037, "Min.Grenze unbekannt");
            ErrorList.Add(1038, "Max.Grenze unbekannt");
            ErrorList.Add(1039, "Notstopp ausgelöst");
            ErrorList.Add(1040, "Endschalter angefahren");
            ErrorList.Add(1041, "Verfahrweg zu klein");
            ErrorList.Add(1042, "Geschwindigkeit zu klein");
            ErrorList.Add(1043, "Ruck zu klein");
            ErrorList.Add(1044, "Kein Trigger Endschalter rein");
            ErrorList.Add(1045, "Kein Trigger Endschalter raus");
            ErrorList.Add(1046, "Fahrweg geclippt");
            ErrorList.Add(1047, "Endschalter überfahren");

            ErrorList.Add(1064, "Wegstrecke zu groß");
            ErrorList.Add(1065, "Bremse und Spannungsversorgung für Endschalter nicht gleichzeitig möglich");
            ErrorList.Add(1066, "Keine Kommutierung nötig");
            ErrorList.Add(1067, "Achse nicht kommutiert");

            ErrorList.Add(1096, "Min.Endschalter aktiv");
            ErrorList.Add(1097, "Max.Endschalter aktiv");
            ErrorList.Add(1098, "Nicht bereit zur Autokommutierung");
            ErrorList.Add(1099, "Kein interpolierender Geber gefunden");
            ErrorList.Add(1100, "I²T Überwachung angesprochen(Langzeit)");
            ErrorList.Add(1101, "I²T Überwachung angesprochen(Kurzzeit)");
            ErrorList.Add(1102, "Überstrom Endstufe");
            ErrorList.Add(1103, "Überstrom beim Einschalten");
            ErrorList.Add(1104, "Überspannung");
            ErrorList.Add(1105, "Sicherung Zwischenkreisspannung defekt");
            ErrorList.Add(1106, "Encoderfehler: Amplitude zu klein");
            ErrorList.Add(1107, "Encoderfehler: Amplitude zu groß");
            ErrorList.Add(1108, "Schleppfehler zu groß");
            ErrorList.Add(1109, "Geschwindigkeit zu groß");
            ErrorList.Add(1110, "Motor blockiert");
            ErrorList.Add(1111, "Motorbremse fehlerhaft");
            ErrorList.Add(1112, "Übertemperatur der Endstufe");
            ErrorList.Add(1113, "Motor überhitzt");
            ErrorList.Add(1114, "Endschalter bei Autokommutierung geschaltet");
            ErrorList.Add(1115, "Lesefehler Temeratur der Endstufe");
            ErrorList.Add(1116, "Zielfenster nicht erreicht");
            ErrorList.Add(1117, "Achse wird Verfahren");
            ErrorList.Add(1118, "Schalter für min.Fahrbereich betätigt");
            ErrorList.Add(1119, "Schalter für max.Fahrbereich betätigt");
            ErrorList.Add(1120, "Zielposition außerhalb min.Fahrbereich");
            ErrorList.Add(1121, "Zielposition außerhalb max.Fahrbereich");
            ErrorList.Add(1122, "Mehrere Endschalter gleichzeitig betätigt");
            ErrorList.Add(1123, "Endstufe durch Hardwareüberwachung ausgeschaltet");
            ErrorList.Add(1124, "Spurfehler Encoder");
            ErrorList.Add(1125, "Amplitude des Endoders zu klein, eventuell kein Geber angeschlossen.");
            ErrorList.Add(1126, "Winkel bei Autokommutierung ausserhalb des Toleranzbereichs, Achse eventuell blockiert");
            ErrorList.Add(1127, "Keine Rundachse");
            ErrorList.Add(1128, "Kein 0 Endschalter / Keine Geber Referenzmarke");
            ErrorList.Add(1129, "Kein Geber Interface");
            ErrorList.Add(1130, "Gebereingang mehrfach zugewiesen");
            ErrorList.Add(1131, "eQep - Gebereingänge nicht konfiguriert(Hardware - Konfiguration MFP)");
            ErrorList.Add(1132, "Zielfenster nicht innerhalb erlaubter Zeit erreicht");
            ErrorList.Add(1133, "Gebereingang nicht verfügbar");
            ErrorList.Add(1134, "Autokommutierungsstrom größer als Nennstrom");
            ErrorList.Add(1135, "Autokommutierungsstrom gleich Null");
            ErrorList.Add(1160, "Dynamische Checksumme des EEProms falsch");
            ErrorList.Add(1161, "Statische Checksumme des EEProms falsch");
            ErrorList.Add(1162, "Falsche EEProm version");
            ErrorList.Add(1163, "EEProm Struktur fehlerhaft");
            ErrorList.Add(1164, "Fenster für Rechenzeit überschritten(500 / 320μs)");
            ErrorList.Add(1165, "Fenster für Rechenzeit überschritten(62, 5 / 40μs)");
            ErrorList.Add(1193, "Warnung: Übertemperatur Endstufe");
            ErrorList.Add(1194, "Warnung: Motortemperatur zu hoch");
            ErrorList.Add(1195, "Treiberspannung unterschritten");
            ErrorList.Add(1196, "Achse deaktiviert");
            ErrorList.Add(1197, "Zwischenkreisspannung zu niedrig");
            ErrorList.Add(1198, "Zwischenkreisspannung zu hoch");
            ErrorList.Add(1250, "Oszilloskop Pretrigger Position größer als Oszilloskop Datengröße");

            ErrorList.Add(4001, "Interner Fehler");
            ErrorList.Add(4002, "Interner Fehler");
            ErrorList.Add(4003, "undefinierter Fehler");
            ErrorList.Add(4004, "unbekannter Schnittstellentyp(kann bei Connect… auftreten)");
            ErrorList.Add(4005, "Fehler beim Initialisieren der Schnittstelle");
            ErrorList.Add(4006, "Keine Verbindung zur Steuerung(z.B.wenn SetPitch vor Connect aufgerufen wird)");
            ErrorList.Add(4007, "Timeout während Lesen von der Steuerung");
            ErrorList.Add(4008, "Fehler bei Befehlsübertragung an die LSTEP");
            ErrorList.Add(4009, "Befehl wurde abgebrochen(mit SetAbortFlag)");
            ErrorList.Add(4010, "Befehl wird von LSTEP nicht unterstützt");
            ErrorList.Add(4011, "Joystick aktiv(kann bei SetJoystickOn / Off auftreten)");
            ErrorList.Add(4012, "Kein Verfahrbefehl möglich, da Joystick aktiv");
            ErrorList.Add(4013, "Regler - Timeout bei Move - Befehl");
            ErrorList.Add(4014, "Fehler beim Kalibrieren, Endschalter nicht korrekt freigefahren");
            ErrorList.Add(4015, "Endschalter in Verfahrrichtung betätigt");
            ErrorList.Add(4016, "Wiederholter Vektorstart!!(Regelung)");
            ErrorList.Add(4031, "Joystick lässt sich nicht einschalten, da move aktiv!");
            ErrorList.Add(4032, "Softwarelimits undefiniert");
        }

        public SMC_LStep_PCIe_Settings Settings
        {
            get
            {
                return (SMC_LStep_PCIe_Settings)settings;
            }
            set
            {
                settings = value;
            }
        }

        public bool IsEnabled { get; set; }

        public enuHWStatus Connect()
        {
            Positioner = new CClassLStep.LStep();
            mHWStatus |= LogError(Positioner.ConnectSimpleW(11, "COM" + Convert.ToString(Settings.COMPort), 115200, true));

            if (File.Exists(Settings.Path))
            {
                mHWStatus |= LogError(Positioner.LoadConfigW(Settings.Path)); // Lade Config Datei
                mHWStatus |= LogError(Positioner.SetControlPars()); // Setzen der Geladenen Parameter in Controller
            }
            else
            {
                mHWStatus |= enuHWStatus.Error;
                log.Add("Unable to load config file: path does not exist!", "ERROR");
                return mHWStatus;
            }

            if (Settings.ReloadConfiguration)
            {
                // mHWStatus |= LogError(Positioner.SaveConfigW()); //als Datei speichern
                // Aktuelle Konfiguration in LSTEP speichern(EEPROM)
                //  mHWStatus |= LogError(Positioner.LStepSave()); // Speichern in EEPROM
                Settings.ReloadConfiguration = false;
            }

            mHWStatus |= LogError(Positioner.SetControllerSteps(Settings.X.FullSteps, Settings.Y.FullSteps, Settings.Z.FullSteps, 200));
            mHWStatus |= LogError(Positioner.SetPitch(Settings.X.Pitch, Settings.Y.Pitch, Settings.Z.Pitch, 1));
            //mHWStatus |= LogError(Positioner.SetPos(0, 0, 0, 0));
            mHWStatus |= LogError(Positioner.SetAxisDirection(0, 0, 0, 1));
            mHWStatus |= LogError(Positioner.SetPowerAmplifier(true)); //Schaltet die Endstufen der Steuerung Ein
            return mHWStatus;
        }

        public enuHWStatus Initialize()
        {
            return mHWStatus;
        }

        public void Release()
        {
            if (Positioner.Disconnect() == 0)
            {
                mHWStatus &= ~enuHWStatus.Ready;
                mHWStatus = enuHWStatus.NotInitialized;
            }
            else
            {
                mHWStatus = enuHWStatus.Error;
            }
        }
        public enuHWStatus HWStatus => mHWStatus;

        enuPositionerStatus IPositioner.Status => mPosStatus;

        public enuPositionerStatus AxisStatus(enuAxes _axis)
        {           
            string strStatus = new String('\0', 256);
            enuPositionerStatus axisState = enuPositionerStatus.Error;

            switch (strStatus.Substring(AxisIndex(_axis), 1))
            {
                case "M":
                    // Achse ist in Bewegung (Motion)
                    axisState = enuPositionerStatus.Busy;
                    break;
                case "-":
                    axisState = enuPositionerStatus.NotInitialized;
                    break;
                case "@":
                    axisState = enuPositionerStatus.Ready;
                    break;
                case "S":
                    // Achse steht in Endschalter
                    axisState = enuPositionerStatus.Error;
                    break;
                case "F":
                    axisState = enuPositionerStatus.Error;
                    break;
                default:
                    axisState = enuPositionerStatus.Error;
                    break;
            }

            return axisState;

            /*
                int intSwitches = 0;
                ErrCheck(Positioner.GetSwitches(ref intSwitches));

                if (intSwitches > 0)
                {
                    return enuPositionerStatus.Error;
                    //todo: log
                }

                if ((intSwitches | 1024) == 1024)
                {
                    ErrCheck(Positioner.RMeasureEx(4));
                }
                else if ((intSwitches | 512) == 512)
                {
                    ErrCheck(Positioner.RMeasureEx(2));
                }
                else if ((intSwitches | 256) == 256)
                {
                    ErrCheck(Positioner.RMeasureEx(1));
                }
                else if ((intSwitches | 4) == 4)
                {
                    ErrCheck(Positioner.CalibrateEx(4));
                }
                else if ((intSwitches | 2) == 2)
                {
                    ErrCheck(Positioner.CalibrateEx(2));
                }
                else if ((intSwitches | 1) == 1)
                {
                    ErrCheck(Positioner.CalibrateEx(1));
                }
                //todo: objLogger.LogEvent(Me, "The stage was moved across the end-switch, stored positions are now invalid!", LL_Warning)

                // return mStatus; //todo:
            }
            */
        }

        public double ValidateDistance(enuAxes _axis, double _distance)
        { //check, if the position can be reached including distance and precision
            return _distance; //todo
        }

        public double ValidatePosition(enuAxes _axis, double _position)
        { //check, if the position can be reached including distance and precision
            return _position; //todo
        }

        public double ValidateSpeed(enuAxes _axis, double _speed)
        {
            switch (_axis)
            {
                case enuAxes.XAxis:
                    if ((_speed <= Settings.X.MaxSpeed) && (_speed > 0))
                    {
                        return _speed;
                    }
                    else
                    {
                        return Settings.X.MaxSpeed;
                    }

                case enuAxes.YAxis:
                    if ((_speed <= Settings.Y.MaxSpeed) && (_speed > 0))
                    {
                        return _speed;
                    }
                    else
                    {
                        return Settings.Y.MaxSpeed;
                    }

                case enuAxes.ZAxis:
                    if ((_speed <= Settings.Z.MaxSpeed) && (_speed > 0))
                    {
                        return _speed;
                    }
                    else
                    {
                        return Settings.Z.MaxSpeed;
                    }
                default: return 0; //todo: log an error event (implement a-axis as well?)
            }
        }

        public double AxisAbsolutePosition(enuAxes _axis)
        {
            double X = 0, Y = 0, Z = 0, A = 0;
            if (mHWStatus == enuHWStatus.Ready)
            {
                mHWStatus = LogError(Positioner.GetPos(ref X, ref Y, ref Z, ref A));
                switch (_axis)
                {
                    case enuAxes.XAxis:
                        return X;
                    case enuAxes.YAxis:
                        return Y;
                    case enuAxes.ZAxis:
                        return Z;
                }
            }
            return 0;
        }

        public double Speed(enuAxes _axis)
        {
            double X = 0, Y = 0, Z = 0, a = 0;
            if (mHWStatus == enuHWStatus.Ready)
            {
                mHWStatus = LogError(Positioner.GetVel(ref X, ref Y, ref Z, ref a));
                switch (_axis)
                {
                    case enuAxes.XAxis:
                        return X;
                    case enuAxes.YAxis:
                        return Y;
                    case enuAxes.ZAxis:
                        return Z;
                    default: return 0;
                }
            }
            return 0;
        }

        public enuPositionerStatus Speed(enuAxes _axis, double _speed)
        {
            if (mPosStatus == enuPositionerStatus.Ready)
            {
                _speed = ValidateSpeed(_axis, _speed);
                mHWStatus = LogError(Positioner.SetVelSingleAxis(AxisIndex(_axis), _speed));
            }
            return mPosStatus;
        }

        public enuPositionerStatus MoveRelativ(enuAxes _axis, double _increment, double _speed)
        {
            if (mPosStatus == enuPositionerStatus.Ready)
            {
                mPosStatus = Speed(_axis, _speed);
                if (mPosStatus == enuPositionerStatus.Ready)
                {
                    mHWStatus = LogError(Positioner.MoveRelSingleAxis(AxisIndex(_axis),
                        ValidateDistance(_axis, _increment), true));
                }
            }
            return mPosStatus;
        }

        public enuPositionerStatus MoveAbsolut(enuAxes _axis, double _position, double _speed)
        {
            if (mPosStatus == enuPositionerStatus.Ready)
            {
                mPosStatus = Speed(_axis, _speed);
                if (mPosStatus == enuPositionerStatus.Ready)
                {
                    mHWStatus = LogError(Positioner.MoveAbsSingleAxis(AxisIndex(_axis),
                        ValidatePosition(_axis, _position), true));
                }
            }
            return mPosStatus;
        }

        private int AxisIndex(enuAxes _axis)
        {
            switch (_axis)
            {
                case enuAxes.XAxis:
                    return Settings.X.AxisNumber;
                case enuAxes.YAxis:
                    return Settings.Y.AxisNumber;
                case enuAxes.ZAxis:
                    return Settings.Z.AxisNumber;
                default:
                    return -1; //todo: log an error event (implement a-axis as well?)
            }
        }

        public enuPositionerStatus AxisStop(enuAxes _axis)
        {
            mHWStatus = LogError(Positioner.StopAxes());
            return mPosStatus;
        }

        public enuPositionerStatus PositionerStatus()
        {
            return mPosStatus;
        }

        public enuPositionerStatus ValidatePosition(ref Position _pos)
        {
            _pos = _pos;
            return enuPositionerStatus.Ready;
        }

        public enuPositionerStatus ValidateSpeeds(ref Position _speed)
        {
            _speed = _speed;
            return enuPositionerStatus.Ready;
        }

        public Position Speeds()
        {
            double X = 0, Y = 0, Z = 0, a = 0;
            mHWStatus = LogError(Positioner.GetVel(ref X, ref Y, ref Z, ref a));
            Position nspeed = new Position(X, Y, Z);
            return nspeed;
        }

        public enuPositionerStatus Speeds(Position _speed)
        {
            if (mPosStatus == enuPositionerStatus.Ready)
            {
                double X = 0, Y = 0, Z = 0, a = 0;
                X = _speed.X;
                Y = _speed.Y;
                Z = _speed.Z;
                mHWStatus = LogError(Positioner.SetVel(X, Y, Z, a));
            }
            return mPosStatus;
        }

        public Position AbsolutePosition()
        {
            double X = 0, Y = 0, Z = 0, a = 0;
            mHWStatus = LogError(Positioner.GetPos(ref X, ref Y, ref Z, ref a));
            Position npos = new Position(X, Y, Z);
            return npos;
        }

        public enuPositionerStatus AbsolutePosition(Position _pos)
        {//TODO: validate and set speeds and distances!
            if (mPosStatus == enuPositionerStatus.Ready)
            {
                // if new Z-Position is higher retract z first
                if (_pos.Z < AbsolutePosition().Z)
                {
                    mHWStatus = LogError(Positioner.MoveAbsSingleAxis(3, _pos.Z, true));
                    if (mPosStatus == enuPositionerStatus.Ready)
                    {
                        mHWStatus = LogError(Positioner.MoveAbs(_pos.X, _pos.Y, _pos.Z, 0, true));
                    }
                }
                else
                {
                    mHWStatus = LogError(Positioner.MoveAbs(_pos.X, _pos.Y, AbsolutePosition().Z, 0, true));
                    if (mPosStatus == enuPositionerStatus.Ready)
                    {
                        mHWStatus = LogError(Positioner.MoveAbsSingleAxis(3, _pos.Z, true));
                    }
                }
            }
            return mPosStatus;
        }

        public enuPositionerStatus RelativePosition(Position _pos)
        {
            return AbsolutePosition(AbsolutePosition().Sum(_pos));
        }

        public enuPositionerStatus StopMovement()
        {
            mHWStatus = LogError(Positioner.StopAxes());
            return mPosStatus;
        }

        private enuHWStatus LogError(long _lngErrCode)
        {
            if (_lngErrCode > 4099)
            {
                _lngErrCode -= 4100;
            }
            if (_lngErrCode != 0)
            {
                mPosStatus = enuPositionerStatus.Error;
                if (ErrorList.ContainsKey(_lngErrCode))
                {
                    log.Add(ErrorList[_lngErrCode], "ERROR");
                }
                return enuHWStatus.Error;
            }
            else
            {
                mPosStatus = enuPositionerStatus.Ready;
                return enuHWStatus.Ready;
            }
        }
        //Transducer

        private void InitTransducerChannels()
        {
            channels = new List<TransducerChannel>();
            channels.Add(new TransducerChannel(this, "X-Axis", "m", enuPrefix.µ, enuChannelType.mixed, enuSensorStatus.OK));
            channels.Add(new TransducerChannel(this, "Y-Axis", "m", enuPrefix.µ, enuChannelType.mixed, enuSensorStatus.OK));
            channels.Add(new TransducerChannel(this, "Z-Axis", "m", enuPrefix.µ, enuChannelType.mixed, enuSensorStatus.OK));
        }
        public enuTransducerType TransducerType => enuTransducerType.Positioner;
        public List<TransducerChannel> Channels { get => channels; }
        public double GetValue(TransducerChannel channel)
        {
            switch (channel.Name)
            {
                case "X-Axis":
                    return AxisAbsolutePosition(enuAxes.XAxis);
                case "Y-Axis":
                    return AxisAbsolutePosition(enuAxes.YAxis);
                case "Z-Axis":
                    return AxisAbsolutePosition(enuAxes.ZAxis);
                default:
                    return 0;
            }
        }
        public double GetAveragedValue(TransducerChannel channel)
        {
            return GetValue(channel);
        }

        public void SetValue(TransducerChannel channel, double _value)
        {
            switch (channel.Name)
            {
                case "X-Axis":
                    MoveAbsolut(enuAxes.XAxis, _value, 1000);
                    break;
                case "Y-Axis":
                    MoveAbsolut(enuAxes.YAxis, _value, 1000);
                    break;
                case "Z-Axis":
                    MoveAbsolut(enuAxes.ZAxis, _value, 1000);
                    break;
                default:
                    break;
            }
        }

        int ITransducer.Averaging
        {
            get => 1;
            set { }
        }
        //Transducer
    }
}
