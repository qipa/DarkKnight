﻿using System;
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

namespace DarkKnight.Network
{
    class Server
    {
        private int client_counter = 1000;

        /// <summary>
        /// The method is responsible for starting a server on a specific port
        /// </summary>
        /// <param name="port">Port number server listen</param>
        /// <exception cref="System.Net.NetworkInformation">Exception is thrown only if passed by parameter port is occupied</exception>
        public void open(int port)
        {
            foreach (TcpConnectionInformation info in (IPGlobalProperties.GetIPGlobalProperties()).GetActiveTcpConnections())
            {
                // check the port is not occupied
                if (info.LocalEndPoint.Port == port)
                    throw new NetworkInformationException();
            }

            // create the socket server
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // serve accept connection for any ip address (local and external) on the port specified
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            // listen max 1000 socket incoming
            server.Listen(1000);
            // call asynchronius new connection
            server.BeginAccept(new AsyncCallback(acceptConnection), server);
        }

        /// <summary>
        /// This method is called when new client connecting to socket
        /// </summary>
        /// <param name="Result"></param>
        private void acceptConnection(IAsyncResult Result)
        {
            // restore the socket object
            Socket server = (Socket)Result.AsyncState;
            // call asynchronius for accept more connections
            server.BeginAccept(new AsyncCallback(acceptConnection), server);

            // add new channel of server and client connected
            ClientListen newConnection = new ClientListen((Socket)server.EndAccept(Result), client_counter++);

            DarkKnightDelegate.callback.newConnection(newConnection);
        }

    }
}