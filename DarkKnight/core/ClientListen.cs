﻿using DarkKnight.core.Clients;
using DarkKnight.Data;
using DarkKnight.Network;
using System;
using System.Net.Sockets;

#region License Information
/* ************************************************************
 * 
 *    @author AntonioJr <antonio@emplehstudios.com.br>
 *    @copyright 2015 Empleh Studios, Inc
 * 
 * 	  Project Folder: https://github.com/antoniojoaojr/DarkKnight
 * 
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion

namespace DarkKnight.core
{
    class ClientListen : Client
    {
        /// <summary>
        /// The buffer to receive data from the client
        /// </summary>
        private byte[] listenBuffer = new byte[65535];

        public ClientListen(Socket socket, int id)
        {
            client = socket;
            _ID = id;

            asyncReceive(this);
        }

        /// <summary>
        /// Is called when this client socket receive a new package
        /// </summary>
        /// <param name="ar"></param>
        private void ReceivablePacket(IAsyncResult ar)
        {
            // getting the current client object
            ClientListen listen = (ClientListen)ar.AsyncState;
            try
            {
                // we calling for handle received in endReceive
                ReceivedHandler(listen, listen.client.EndReceive(ar));
            }
            catch
            {
                listen.Close();
            }
        }

        private void ReceivedHandler(ClientListen listen, int size)
        {
            // get the byte array with the size received
            byte[] received = getReceivedPacket(listen.listenBuffer, size);

            // after restore the client object and also the received packet in original size, 
            // we release the client asynchronously to receive more packages
            // so we can optimize the delivery time of the package when many data are sent in a short time
            // we if this client socket is defined
            if (listen.socketLayer != SocketLayer.undefined)
                asyncReceive(listen);

            // from here we are already processing the package without worrying that we are delaying the arrival of new

            // register the queue information registration of client
            Registers(listen);

            // if size is zero, we do not continue sense, just abandon this method and release the thread
            if (size == 0)
                return;

            ClientWork.udpate(listen);

            // if the SocketLayer of this client is defined just we handle the packet
            if (listen.socketLayer != SocketLayer.undefined)
            {
                if (IsPing(received))
                    return;

                byte[] decoded;
                if (listen.socketLayer == SocketLayer.websocket)
                    decoded = listen._decode(PacketWeb.decode(received));
                else
                    decoded = listen._decode(received);

                listen.toApplication(listen, new PacketHandler(decoded));

                return;
            }

            // define the socketLayer of this client
            byte[] webSocket = PacketWeb.auth(received);
            // if the webpacket response length is bigger than 1, the socket is 'websocket'
            if (webSocket.Length > 1)
            {
                listen.socketLayer = SocketLayer.websocket;
                // when working with websocket have to do the handshake on the connection, 
                // here we have the using Authentication processed and sent to the websocket finalizing the handshake
                listen.client.Send(webSocket);
            }
            else
            {
                // otherwise the socket is normal 'socket' layer
                listen.socketLayer = SocketLayer.socket;
                // this response is finalizing handshake for normal socket layer
                listen.client.Send(new byte[] { 32, 32 });
            }

            // if we come here is because it was the first received packet,
            // we are sure that we've set the type of layer of SocketLayer that our client is,
            // so we will notify the application which new is connected
            Application.send(ApplicationSend.connectionOpened, new object[] { listen });

            // we release the cliente to receive packet
            asyncReceive(listen);
        }

        private bool IsPing(byte[] received)
        {
            if (received.Length != 4)
                return false;

            if ((received[0] + received[1] + received[2] + received[3]) != 398)
                return false;

            return true;
        }

        /// <summary>
        /// Restore registor informed by the application, to store object of client
        /// </summary>
        /// <param name="listen"></param>
        private void Registers(ClientListen listen)
        {
            // we try get a dequeue of registration
            RegisterAbstract register = Register.GetValue(listen._ID);
            // while dequeue of registration not null
            // we make this
            while (register != null)
            {
                // restore the object type
                RegisterType type = register.getType;
                // make selection by type of object
                switch (type)
                {
                    case RegisterType.crypt:
                        // if is crypt, register the crypt in the client
                        listen._registerCrypt(register.getAbstract<Object>());
                        break;
                }

                // try get a dequeue again of registration
                register = Register.GetValue(listen._ID);
            }
        }


        /// <summary>
        /// Send the packet to the server appliaction handler and process
        /// </summary>
        /// <param name="packet">Packet to send</param>
        private void toApplication(ClientListen listen, Packet packet)
        {
            // we make a finally validation of the packet in the server
            // if the packet is invalid, just print a log in the output
            if (packet.format.getStringFormat == "???" && packet.data.Length == 0)
            {
                DarkKnight.Utils.Log.Write("Client [" + listen.IPAddress.ToString() + " - " + listen.Id + "] sended a invalid package format [???] with no data");
                return;
            }

            Application.send(ApplicationSend.ReceivedPacket, new object[] { listen, packet });
        }

        private byte[] getReceivedPacket(byte[] buffer, int size)
        {
            lock (buffer)
            {
                // security of return if the size received is zero
                if (size == 0)
                    return new byte[1];

                byte[] pack = new byte[size];

                Array.Copy(buffer, pack, size);

                return pack;
            }
        }

        /// <summary>
        /// Start listen this client socket to receive package async
        /// </summary>
        private void asyncReceive(ClientListen listen)
        {
            try
            {
                // we try to continue receiving client packages
                listen.client.BeginReceive(listen.listenBuffer, 0, listen.listenBuffer.Length, SocketFlags.None, new AsyncCallback(listen.ReceivablePacket), listen);
            }
            catch
            {
                // if we have an exception to receive a new package, it means that we have lost the connection with the client
                // notify this to the application
                listen.Close();
            }
        }
    }
}
