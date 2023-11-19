//tabs=4
// --------------------------------------------------------------------------------
// Simple Temp/Humidity/Pressure sensor replacing BlueAstro WeatherStick
//
// ASCOM ObservingConditions driver for MiniWeather
//
// Description:	An extremely simple hardware solution, using Arduino Mini Pro, BME280 sensor and FTDi serial cable
//
// Implements:	ASCOM ObservingConditions interface version: <To be completed by driver developer>
// Author:		(CJW) Chris Woodhouse 2021 <chris@digitalastrophotography.co.uk>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 03-Feb-2021	CJW	1.0.0	Initial edit, created from ASCOM driver template
// 04-Feb-2021  CJW 1.1.0   Added trim values to ASCOM driver
// 07-Feb-2021  CJW 2.1     use dewpoint calculation from updated trimmed values
// 07-Mar-2021  CJW 2.2     put back in DTR on first connect
// 15-Mar-2021  CJW 2.3     put in more robust connect strategy, detecting no connection
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define ObservingConditions

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ASCOM.MiniWeather
{
    //
    // Your driver's DeviceID is ASCOM.MiniWeather.ObservingConditions
    //
    // The Guid attribute sets the CLSID for ASCOM.MiniWeather.ObservingConditions
    // The ClassInterface/None attribute prevents an empty interface called
    // _MiniWeather from being created and used as the [default] interface
    //

    /// <summary>
    /// ASCOM ObservingConditions Driver for MiniWeather.
    /// </summary>
    enum MessageIndex { tempC, humdity, pressure}
    [Guid("47f97e1a-c38d-4fce-a51e-44ae17f4682c")]
    [ClassInterface(ClassInterfaceType.None)]
    public class ObservingConditions : IObservingConditions
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.MiniWeather.ObservingConditions";
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "MiniWeather ASCOM driver";

        internal static string comPortProfileName = "COM Port"; // Constants used for Profile persistence
        internal static string comPortDefault = "COM1";
        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "false";

        internal static string comPort; // Variables to hold the current device configuration
        internal static double humidtrim, temptrim, pressuretrim;  // sensor output trim values
        internal string buffer = "";
        internal bool startCharRx;  // indicates start of message detected
        internal char endChar = '#';  // end character of Arduino response
        internal char startChar = '$';  // start character of Arduino response
        //internal char[] delimeter = new char[] {','};  // delimiter between Arduino response values
        internal char[] delimeter = { ',' };
        internal string arduinoMessage = ""; // string that builds up received message
        internal bool dataRx = false;  // indicates that data is available to read
        //  these values may be populated by setup chooser screen in the future
        string[] configure = new string[3]; // for adjustments from setup chooser screen

        // key global varibles and default values
        internal double tempC = 20.0;  // bme sensor ambient
        internal double humidity = 50.0;  // default humidity
        internal double pressure = 1000.0;
        internal double adjustedTempC = 10; // adjusted value (used in RH calculation)
        internal double adjustedHumidity = 50;  //calculated humidity with profile offset
        internal double adjustedPressure = 1000;  // for the hell of it
        internal double dewpoint = 0; // dewpoint (calculated)
        internal double period = 0.02;   // measurement period (hours, worse case)

        // path name for storing text file
        private string[] MiniStatus = new string[3];  //  "$tempC, humidity, pressure#"

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;
        private SerialPort Serial;  // for communications

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniWeather"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public ObservingConditions()
        {
            tl = new TraceLogger("", "MiniWeather");
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("ObservingConditions", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro-utilities object
            Serial = new SerialPort();  // standard .net serial port

            tl.LogMessage("ObservingConditions", "Completed initialisation");
        }

        // openArduino initilises serial port and set up an event handler to suck in characters
        // it runs in the background, Arduino broadcasts every 2 seconds
        // initializes message status and contents
        private bool openArduino()
        {
            for (int i = 0; i < 3; i++) MiniStatus[i] = "1.0";  // safe default serial string values
            Serial.BaudRate = 9600;
            Serial.PortName = comPort;
            Serial.Parity = Parity.None;
            Serial.DataBits = 8;
            Serial.Handshake = System.IO.Ports.Handshake.None;
            Serial.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(receiveData);
            Serial.ReceivedBytesThreshold = 1;
            Serial.DtrEnable = true;  // required to reset the Arduino for comms 
            try
            {
                Serial.Open();              // open port
                Serial.DiscardInBuffer();   // and clear it out just in case
                dataRx = false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        // receiveData is based on a code fragment suggested by Per and reads characters as they arrive
        // it decodes the messages, looking for framing characters and then splits the CSV string into
        // component parts to represent the status flags from the Arduino 
        private void receiveData(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == System.IO.Ports.SerialData.Chars)
            {
                while (Serial.BytesToRead > 0)
                {
                    char c = (char)Serial.ReadChar();  // wait for start character
                    if (!startCharRx)
                    {
                        if (c == startChar)  // and then initialise the message
                        {
                            startCharRx = true;
                            buffer = "";  // clear buffer
                        }
                    }
                    else
                    {
                        if (c == endChar)
                        {
                            arduinoMessage = buffer;  // transfer the buffer to the message and clear the buffer
                            buffer = "";
                            startCharRx = false;
                            if (arduinoMessage.Length <= 25) // check the message length is OK  38 is longest, below zero and high pressure
                            {
                                dataRx = true; // tell the world that data is available
                                MiniStatus = arduinoMessage.Split(delimeter);  // temp, humidity, pressure
                                tl.LogMessage("communications", arduinoMessage);
                                populate();
                            }
                            else  // message was corrupted
                            {
                                dataRx = false;
                                tl.LogMessage("communications", "corrupted message length");
                                arduinoMessage = "";
                            }
                        }
                        else
                        {
                            buffer += c;  // build up message string in buffer
                        }
                    }
                }
            }
        }
        private void populate()  // update more convenient global variables from incoming stream
        {        
            tempC = double.Parse(MiniStatus[(int)MessageIndex.tempC]);
            humidity = double.Parse(MiniStatus[(int)MessageIndex.humdity]);
            pressure = double.Parse(MiniStatus[(int)MessageIndex.pressure]);

            // and the adjusted values
            adjustedHumidity = humidity + humidtrim;
            adjustedTempC = tempC + temptrim; 
            adjustedPressure = pressure + pressuretrim;
            // calculate dewpoint from adjusted temperature and humidity
            double vp; 
            vp = adjustedHumidity * 0.01 * 6.112 * Math.Exp((17.62 * adjustedTempC) / (adjustedTempC + 243.12));
            dewpoint = (243.12 * Math.Log(vp) - 440.1) / (19.43 - (Math.Log(vp)));
        }

        //
        // PUBLIC COM INTERFACE IObservingConditions IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupMiniWeather F = new SetupMiniWeather(tl))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            throw new ASCOM.MethodNotImplementedException("CommandBlind");

        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            string ret = CommandString(command, raw);
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the trace logger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
            Serial.Dispose();
            Serial = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    LogMessage("Connected Set", "Connecting to port {0}", comPort);
                    if (!openArduino())
                    {
                        Serial.Dispose();
                        Serial.DiscardInBuffer();
                        dataRx = false;
                        LogMessage("connection", "Problem with connecting", comPort);
                    }
                    else  // check that it is receiving data
                    {
                        var t = Task.Run(async delegate { await Task.Delay(600); });
                        int counter = 0;
                        while (!dataRx || counter < 10)
                        {
                            t.Wait();
                            counter++;
                        }
                        if (!dataRx)
                        {
                            connectedState = false;
                            Serial.Dispose();  //disconnect to serial
                            Serial.DiscardInBuffer();
                            throw new ASCOM.NotConnectedException("no transmission!");
                        }
                        else connectedState = true;
                    }
                }
                else
                {
                    connectedState = false;
                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);
                    Serial.Dispose();  //disconnect to serial
                }
            }
        }

        public string Description
        {
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "1");
                return Convert.ToInt16("1");
            }
        }

        public string Name
        {
            get
            {
                string name = "Mini Weather";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region IObservingConditions Implementation

        /// <summary>
        /// Gets and sets the time period over which observations wil be averaged
        /// </summary>
        /// <remarks>
        /// Get must be implemented, if it can't be changed it must return 0
        /// Time period (hours) over which the property values will be averaged 0.0 =
        /// current value, 0.5= average for the last 30 minutes, 1.0 = average for the
        /// last hour
        /// </remarks>
        public double AveragePeriod
        {
            get
            {
                LogMessage("AveragePeriod", "get - 0");
                return period;
            }
            set
            {
                if ((value < 0) || (value > 5.0)) throw new ASCOM.InvalidValueException("AveragePeriod(" + value + ")");
                else period = value;
            }
        }

        /// <summary>
        /// Amount of sky obscured by cloud
        /// </summary>
        /// <remarks>0%= clear sky, 100% = 100% cloud coverage</remarks>
        public double CloudCover
        {
            get
            {
                LogMessage("CloudCover", "get - not implemented");
                throw new PropertyNotImplementedException("CloudCover", false);
            }
        }

        /// <summary>
        /// Atmospheric dew point at the observatory in deg C
        public double DewPoint
        {
            get
            {
                return dewpoint;
            }            
        }

        /// <summary>
        /// Atmospheric relative humidity at the observatory in percent
        public double Humidity
        {
            get
            {
                return adjustedHumidity;
            }
        }

        /// <summary>
        /// Atmospheric pressure at the observatory in hectoPascals (mB)
        public double Pressure
        {
            get
            {
                return adjustedPressure;
            }
        }

        /// <summary>
        /// Rain rate at the observatory
        /// </summary>
        /// <remarks>
        /// This property can be interpreted as 0.0 = Dry any positive nonzero value
        /// = wet.
        /// </remarks>
        public double RainRate
        {
            get
            {
                LogMessage("RainRate", "get - not implemented");
                throw new PropertyNotImplementedException("RainRate", false);
            }
        }

        /// <summary>
        /// Forces the driver to immediatley query its attached hardware to refresh sensor
        /// values
        /// </summary>
        public void Refresh()
        {
            throw new ASCOM.MethodNotImplementedException();
        }

        /// <summary>
        /// Provides a description of the sensor providing the requested property
        /// </summary>
        /// <param name="PropertyName">Name of the property whose sensor description is required</param>
        /// <returns>The sensor description string</returns>
        public string SensorDescription(string PropertyName)
        {
            switch (PropertyName.Trim().ToLowerInvariant())
            {
                case "averageperiod":
                    return "Average period in hours, immediate values are only available";
                case "dewpoint":
                    return "dewpoint";
                case "humidity":
                    return "RH%";
                case "pressure":
                    return "hPa";
                case "rainrate":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "skybrightness":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "skyquality":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "starfwhm":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "skytemperature":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "temperature":
                    return "temp C";
                case "winddirection":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "windgust":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "windspeed":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                case "cloudcover":
                    LogMessage("SensorDescription", "{0} - not implemented", PropertyName);
                    throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                default:
                    LogMessage("SensorDescription", "{0} - unrecognised", PropertyName);
                    throw new ASCOM.InvalidValueException("SensorDescription(" + PropertyName + ")");
            }
        }

        /// <summary>
        /// Sky brightness at the observatory
        /// </summary>
        public double SkyBrightness
        {
            get
            {
                LogMessage("SkyBrightness", "get - not implemented");
                throw new PropertyNotImplementedException("SkyBrightness", false);
            }
        }

        /// <summary>
        /// Sky quality at the observatory
        /// </summary>
        public double SkyQuality
        {
            get
            {
                LogMessage("SkyQuality", "get - not implemented");
                throw new PropertyNotImplementedException("SkyQuality", false);
            }
        }

        /// <summary>
        /// Seeing at the observatory
        /// </summary>
        public double StarFWHM
        {
            get
            {
                LogMessage("StarFWHM", "get - not implemented");
                throw new PropertyNotImplementedException("StarFWHM", false);
            }
        }

        /// <summary>
        /// Sky temperature at the observatory in deg C
        /// </summary>
        public double SkyTemperature
        {
            get
            {
                LogMessage("SkyTemperature", "get - not implemented");
                throw new PropertyNotImplementedException("SkyTemperature", false);
            }
        }

        /// <summary>
        /// Temperature at the observatory in deg C
        /// </summary>
        public double Temperature
        {
            get
            {
                return adjustedTempC;
            }
        }

        /// <summary>
        /// Provides the time since the sensor value was last updated
        /// </summary>
        /// <param name="PropertyName">Name of the property whose time since last update Is required</param>
        /// <returns>Time in seconds since the last sensor update for this property</returns>
        /// <remarks>
        /// PropertyName should be one of the sensor properties Or empty string to get
        /// the last update of any parameter. A negative value indicates no valid value
        /// ever received.
        /// </remarks>
        public double TimeSinceLastUpdate(string PropertyName)
        {
            // the checks can be removed if all properties have the same time.
            if (!string.IsNullOrEmpty(PropertyName))
            {
                switch (PropertyName.Trim().ToLowerInvariant())
                {
                    // break or return the time on the properties that are implemented
                    case "averageperiod":
                        return 0.1;
                    case "dewpoint":
                        return 10;
                    case "humidity":
                        return 10;
                    case "pressure":
                        return 10;
                    case "rainrate":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "skybrightness":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "skyquality":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "starfwhm":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "skytemperature":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "temperature":
                        return 15;
                    case "winddirection":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "windgust":
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "windspeed":
                        // throw an exception on the properties that are not implemented
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    case "cloudcover":
                        // throw an exception on the properties that are not implemented
                        LogMessage("TimeSinceLastUpdate", "{0} - not implemented", PropertyName);
                        throw new MethodNotImplementedException("SensorDescription(" + PropertyName + ")");
                    default:
                        LogMessage("TimeSinceLastUpdate", "{0} - unrecognised", PropertyName);
                        throw new ASCOM.InvalidValueException("SensorDescription(" + PropertyName + ")");
                }
            }
            return 10;
        }

        /// <summary>
        /// Wind direction at the observatory in degrees
        /// </summary>
        /// <remarks>
        /// 0..360.0, 360=N, 180=S, 90=E, 270=W. When there Is no wind the driver will
        /// return a value of 0 for wind direction
        /// </remarks>
        public double WindDirection
        {
            get
            {
                LogMessage("WindDirection", "get - not implemented");
                throw new PropertyNotImplementedException("WindDirection", false);
            }
        }

        /// <summary>
        /// Peak 3 second wind gust at the observatory over the last 2 minutes in m/s
        /// </summary>
        public double WindGust
        {
            get
            {
                LogMessage("WindGust", "get - not implemented");
                throw new PropertyNotImplementedException("WindGust", false);
            }
        }

        /// <summary>
        /// Wind speed at the observatory in m/s
        /// </summary>
        public double WindSpeed
        {
            get
            {
                LogMessage("WindSpeed", "get - not implemented");
                throw new PropertyNotImplementedException("WindSpeed", false);
            }
        }

        #endregion

        #region private methods

        #region calculate the gust strength as the largest wind recorded over the last two minutes

        // save the time and wind speed values
        private Dictionary<DateTime, double> winds = new Dictionary<DateTime, double>();

        private double gustStrength;

        private void UpdateGusts(double speed)
        {
            Dictionary<DateTime, double> newWinds = new Dictionary<DateTime, double>();
            var last = DateTime.Now - TimeSpan.FromMinutes(2);
            winds.Add(DateTime.Now, speed);
            var gust = 0.0;
            foreach (var item in winds)
            {
                if (item.Key > last)
                {
                    newWinds.Add(item.Key, item.Value);
                    if (item.Value > gust)
                        gust = item.Value;
                }
            }
            gustStrength = gust;
            winds = newWinds;
        }

        #endregion

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "ObservingConditions";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()  // ASCOM profile for storing settings
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "ObservingConditions";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
                humidtrim = double.Parse(driverProfile.GetValue(driverID, "humid trim", string.Empty, "0"));
                temptrim = double.Parse(driverProfile.GetValue(driverID, "temp trim", string.Empty, "0"));
                pressuretrim = double.Parse(driverProfile.GetValue(driverID, "pressure trim", string.Empty, "0"));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "ObservingConditions";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
                driverProfile.WriteValue(driverID, "humid trim", humidtrim.ToString());
                driverProfile.WriteValue(driverID, "temp trim", temptrim.ToString());
                driverProfile.WriteValue(driverID, "pressure trim", pressuretrim.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        #endregion
    }
}
