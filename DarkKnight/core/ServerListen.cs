﻿using DarkKnight.Network;
using System;
using System.Net;
using System.Net.NetworkInformation;
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
    class ServerListen
    {
        private int nextClientId = 1000;
        
        /// <summary>
        /// The method is responsible for starting a server on a specific port
        /// </summary>
        /// <param name="port">Port number server listen</param>
        /// <exception cref="System.Net.NetworkInformation">Exception is thrown only if passed by parameter port is occupied</exception>
        public void open()
        {
            foreach (TcpConnectionInformation info in (IPGlobalProperties.GetIPGlobalProperties()).GetActiveTcpConnections())
            {
                // check the port is not occupied
                if (info.LocalEndPoint.Port == ServerController.config.Port)
                    throw new NetworkInformationException();
            }

            // create the socket object to accept tcp stream
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // put the socket to accept connections from any IP on the specified port
            server.Bind(new IPEndPoint(IPAddress.Any, ServerController.config.Port));
            // we setting maximum number of sockets in line to enter the server
            server.Listen(ServerController.config.Backlog);
            // begin to list new connections asynchronous
            server.BeginAccept(new AsyncCallback(acceptConnection), server);
            // sets the socket is running
            ServerController.setWork(server);
        }

        /// <summary>
        /// This method is called when new client connecting to socket
        /// </summary>
        /// <param name="Result"></param>
        private void acceptConnection(IAsyncResult Result)
        {
            // restore the socket object
            Socket server = (Socket)Result.AsyncState;

            try
            {
                // get socket of client connected
                Socket client = server.EndAccept(Result);

                // one thread per time
                lock (ThreadLocker.sync("ServerListen::acceptConnection"))
                {
                    nextClientId++;
                }

                if (UnauthenticatedTable.get(((IPEndPoint)client.RemoteEndPoint).Address.ToString()) >= ServerController.config.MaxUnauthenticatedAccept)
                {
                    client.Close();
                }
                else {
                    // we add the new connected client to the listener channel
                    new ClientListen(client, nextClientId);
                }
            }
            catch (Exception ex)
            {
                DarkKnight.Utils.Log.Write("A new client connected generate a error and not accepted: \n" + ex.Message + " - " + ex.StackTrace, Utils.LogLevel.ERROR);
            }
            finally
            {
                // release to accept more connections asynchronous
                server.BeginAccept(new AsyncCallback(acceptConnection), server);
            }
        }
    }
}
