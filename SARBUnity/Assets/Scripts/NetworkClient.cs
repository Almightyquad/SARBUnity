﻿using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class NetworkClient : MonoBehaviour
{

    // String and Host name
    public String hostname;                     // Ip of the host:      Local or Public ip depends.
    public Int32 port;                          // Port of the host 

    internal Boolean socketReady = false;
    internal String inputBuffer = "";

    TcpClient tcpSocket;
    NetworkStream netStream;

    public static NetworkClient instance = null;
    StreamWriter streamWriter;
    StreamReader streamReader;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }



    public void updateHostName(string hostname)
    {
        this.hostname = hostname;
    }



    public void updatePort(Int32 port)
    {
        this.port = port;
    }



    public void initTcpClient()
    {
        try
        {
            Debug.Log(this.hostname + " " + this.port);
            this.tcpSocket = new TcpClient(this.hostname, this.port);
            this.netStream = this.tcpSocket.GetStream();
            this.streamWriter = new StreamWriter(this.netStream);
            this.streamReader = new StreamReader(this.netStream);
            this.socketReady = true;
            Debug.Log("Connected to: " + this.hostname + " Port: " + this.port);
        }

        catch (Exception e)
        {
            // Something went wrong
            Debug.Log("Socket error: " + e);
        }
    }

    public void writeSocket(string line)
    {
        if (!this.socketReady)
            return;

        // Consider a string builder for better performance here
        string datasize = "000000000|0000";
        char[] tempArray = datasize.ToCharArray();
        string sizeOfMessage = line.Length.ToString();
        string sizeOfCommand = "1";

        int offset = sizeOfCommand.Length - 1;
        for (int i = datasize.Length - 1; i > ((datasize.Length - 1) - sizeOfCommand.Length); i--)
        {
            tempArray[i] = sizeOfCommand.ToCharArray()[offset];
            offset--;
        }

        offset = sizeOfMessage.Length - 1;
        for (int i = datasize.Length-6; i > ((datasize.Length-6) - sizeOfMessage.Length); i--)
        {
            tempArray[i] = sizeOfMessage.ToCharArray()[offset];
            offset--;
        }

       
        datasize = new string(tempArray);


        // Send the header
        ASCIIEncoding asen = new ASCIIEncoding();
        Byte[] Buffer = asen.GetBytes(datasize);
        netStream.Write(Buffer, 0, Buffer.Length);

        // send the data
        Buffer = asen.GetBytes(line);
        netStream.Write(Buffer, 0, Buffer.Length);


    }

public string receiveSocket()
{
        string message = "";
        if (!this.socketReady)
            return message;

        // Read data
        if (netStream.DataAvailable)
        {

          // Read the Header
           int packageSize = 0;
           int packageCommand = 0;
           readHeader(ref packageSize, ref packageCommand);

            if(packageCommand == 1)
            {
              message =  readEcho(packageSize);
            }

            // read the heightmap
            if (packageCommand == 2)
            {
                readHeightMap(packageSize);
            }

            //for (int i = 0; i < dataBuffer.Length; i++)
            //{
            //    message = message + "" + (Convert.ToChar(dataBuffer[i]).ToString());
            //}
           // Debug.Log("MESSAGE " + message);

            return message;
        }
        String noMessage = "";
        return noMessage;
}


    // Close the sockets and readers
    
    public void closeSocket()
    {
        if (!this.socketReady)
            return;

        this.streamWriter.Close();
        this.streamReader.Close();
        this.tcpSocket.Close();
        this.socketReady = false;
    }

    private Byte[] readData(int size)
    {
        Byte[] buffer = new Byte[size];
        while (size>0)
        {
           int bytes = netStream.Read(buffer, 0, size);
           size -= bytes;
        }
        
        return buffer;
    }

    private void readHeader(ref int packageSize,  ref int packageCommand)
    {

        // Read header
        byte[] header = new byte[14];
        int size = netStream.Read(header, 0, 14);


        char[] array = new char[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = Convert.ToChar(header[i]);
        }


        packageSize = Convert.ToInt32(parseheader(array, 0, 9));
        packageCommand = Convert.ToInt32(parseheader(array, 10, 14));

        Debug.Log("PackageSize: " + packageSize + "      " + "PackageCommand: " + packageCommand);

    }


    private string parseheader(char[] array, int start, int end)
    {
        string temp = "";

       
        for (int i = start; i < end; i++)
        {
            temp = temp + array[i].ToString();
        }

        return temp;
    }

    private void readHeightMap(int packageSize)
    {
        string message = "";

        if (packageSize > 0)
        {
            int storageSize = 2048;
            Byte[] storageBuffer = new Byte[storageSize];

            // Reading heightmap
            do
            {
                int readSize = 0;
                if (storageSize < packageSize)
                {
                    readSize = storageBuffer.Length;
                }

                else
                {
                    readSize = packageSize;
                }

                storageBuffer = readData(readSize);

                for (int i = 0; i < storageBuffer.Length; i++)
                {
                    message = message + "" + (Convert.ToChar(storageBuffer[i]).ToString());
                }
                Debug.Log("MESSAGE " + message);
                message = "";
                packageSize -= readSize;


            }
            while (packageSize > 0);
        }
    }



    private string readEcho(int packageSize)
    {
        string echo = "";
        if (packageSize > 0)
        {
            int storageSize = 2048;
            Byte[] storageBuffer = new Byte[storageSize];

            // Reading heightmap
            do
            {
                int readSize = 0;
                if (storageSize < packageSize)
                {
                    readSize = storageBuffer.Length;
                }

                else
                {
                    readSize = packageSize;
                }

                storageBuffer = readData(readSize);

                for (int i = 0; i < storageBuffer.Length; i++)
                {
                    echo = echo + "" + (Convert.ToChar(storageBuffer[i]).ToString());
                }
                packageSize -= readSize;


            }
            while (packageSize > 0);
        }

        return echo;
    }

}
