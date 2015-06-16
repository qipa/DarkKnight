﻿using DarkKnight;
using System;

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

namespace ChatServerApp
{
    class ChatService : DKService
    {
        public override void newConnection(DarkKnight.Network.DKClient client)
        {
            throw new NotImplementedException();
        }

        public override void newPacket(DarkKnight.Network.DKClient client, DKBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public override void connectionClosed(DarkKnight.Network.DKClient client)
        {
            throw new NotImplementedException();
        }

    }
}
