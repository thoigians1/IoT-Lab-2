using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using M2MqttUnity;
using Newtonsoft.Json.Linq;
using System.Linq ;
using Newtonsoft.Json;

[Serializable]
public class DataControl {
    public string device;
    public string status;

    public DataControl(string name, string isOn)
    {
        device = name;
        status = isOn;
    }
}



[Serializable]
public class Status_Data
    {
        public string temperature {get; set;}
        public string humidity {get; set;}
    }
[Serializable]
public class Led_Data
    {
        public string device = "LED";
        public string status = "OFF";
    }
[Serializable]
public class Pump_Data
    {
        public string device = "PUMP";
        public string status = "OFF";
    }

public class MqttUnityClient : M2MqttUnityClient
{
    public Text error;
    public Text temperatureValue;
    public Text humidityValue;
    public Toggle ledToggle;
    public Toggle pumpToggle;

    public InputField addressInputField;
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Button connectButton;



    public List<string> topics = new List<string>();
    private bool updateUI = false;

    public void PublishTopics(String topic, String msg)
    {
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        // Debug.Log("Publishing " + msg);
        error_msg("Test message published.");
    }

    public void Change_Led()
    {
        string msg = JsonUtility.ToJson(new DataControl("LED", this.ledToggle.isOn ? "ON" : "OFF"));
        PublishTopics(topics[1], msg);
    }

    public void Change_Pump()
    {
        string msg = JsonUtility.ToJson(new DataControl("PUMP", this.pumpToggle.isOn ? "ON": "OFF"));
        PublishTopics(topics[2], msg);
    }

    public void SetBrokerAddress(string brokerAddress)
    {
        if (addressInputField && !updateUI)
        {
            this.brokerAddress = brokerAddress;
        }
    }

    public void SetUsername(string username)
    {
        if (usernameInputField && !updateUI)
        {
            this.mqttUserName = username;
        }
    }
    public void SetPassword(string password)
    {
        if (passwordInputField && !updateUI)
        {
            this.mqttPassword = password;
        }
    }

    public void Message(string msg)
    {
        if (msg != null)
        {
            error.text = msg;
            updateUI = true;
        }
    }

    public void error_msg(string msg)
    {
        if (error != null)
        {
            error.text += msg + "\n";
            updateUI = true;
        }
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
        Message("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        Message("Connected to broker on " + brokerAddress + "\n");
        SubscribeTopics();
        
        // client.Publish(topics[0], System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new DataStatus(31f, 70f))), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

        SwitchScene();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { topics[0] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { topics[0] }); 
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        error_msg("CONNECTION FAILED! " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        error_msg("Disconnected.");
    }

    protected override void OnConnectionLost()
    {
        error_msg("CONNECTION LOST!");
    }

    private void UpdateUI()
    {
        if (client == null)
        {
            if (connectButton != null)
            {
                connectButton.interactable = true;
            }
        }
        else
        {
            if (connectButton != null)
            {
                connectButton.interactable = !client.IsConnected;
            }
        }
        if (addressInputField != null && connectButton != null)
        {
            addressInputField.interactable = connectButton.interactable;
            addressInputField.text = brokerAddress;
        }
        if (usernameInputField != null && connectButton != null)
        {
            usernameInputField.interactable = connectButton.interactable;
            usernameInputField.text = mqttUserName;
        }
        if (passwordInputField != null && connectButton != null)
        {
            passwordInputField.interactable = connectButton.interactable;
            passwordInputField.text = mqttPassword;
        }
        updateUI = false;
    }

    protected override void Start()
    {
        updateUI = true;
        base.Start();
    }

    public void connectBtnOnClickStart()
    {
        autoConnect = true;
        SetBrokerAddress(this.addressInputField.text);
        SetUsername(this.usernameInputField.text);
        SetPassword(this.passwordInputField.text);
        base.Start();
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg);
        ProcessMessageStatus(msg);
    }

    private void ProcessMessageStatus(string msg){
        Status_Data Status_Data = JsonConvert.DeserializeObject<Status_Data>(msg);
        Debug.Log(Status_Data);
        float value = float.Parse(Status_Data.temperature);  
        this.temperatureValue.text = Status_Data.temperature + "°C";
        float value1 = float.Parse(Status_Data.humidity);
        this.humidityValue.text = Status_Data.humidity + "%";

    }


    protected override void Update()
    {
        base.Update(); 
        if (updateUI)
        {
            UpdateUI();
        }

        if (updateScene == true)
        {
            SwitchOut(canvasScene1);
            SwitchIn(canvasScene2);
        }
        else if (updateScene == false)
        {
            SwitchOut(canvasScene2);
            SwitchIn(canvasScene1);
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnValidate()
    {
    }


    public CanvasGroup canvasScene1;
    public CanvasGroup canvasScene2;
    public int offsetPositionX;
    public int offsetPositionY;
    public float speed = 1.0F;
    private float startTime;
    private bool updateScene = false;

    void SwitchIn(CanvasGroup _canvas)
    {
        Vector3 targetPos = new Vector3(0, offsetPositionY ,0);
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / offsetPositionX;
        _canvas.transform.localPosition = Vector3.Lerp(_canvas.transform.localPosition, targetPos, fractionOfJourney);
    }
    void SwitchOut(CanvasGroup _canvas)
    {
        Vector3 targetPos;
        if (string.Equals(_canvas.name, "Login"))
            targetPos = new Vector3(-offsetPositionX, offsetPositionY, 0);
        else
            targetPos = new Vector3(offsetPositionX, offsetPositionY, 0);
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / offsetPositionX;
        _canvas.transform.localPosition = Vector3.Lerp(_canvas.transform.localPosition, targetPos, fractionOfJourney);
    }

    void SwitchScene()
    {
        startTime = Time.time;
        updateScene = !updateScene;
        if (canvasScene1.interactable == true)
        {
            canvasScene1.interactable = false;
            canvasScene1.blocksRaycasts = false;
            canvasScene2.interactable = true;
            canvasScene2.blocksRaycasts = true;
        }
        else
        {
            canvasScene2.interactable = false;
            canvasScene2.blocksRaycasts = false;
            canvasScene1.interactable = true;
            canvasScene1.blocksRaycasts = true;
        }
    }
    
}
