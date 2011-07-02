/**	KinectDaemon Serialization Utilities
		
	@author Unknown, pulled from tutorial on web.
	@website http://www.seethroughskin.com/blog/?p=1159
*/

/* 
 * This program is free software; you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation; either version 2 of the License, or 
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
 * for more details.
 * 
 * You should have received a copy of the GNU General Public License along 
 * with this program; if not, write to the Free Software Foundation, Inc., 
 * 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 */
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KinectDaemon
{
    public class SerializationUtils
    {
        public static byte[] SerializeToByteArray(object request)
        {
            byte[] result;
            BinaryFormatter serializer = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream())
            {
                serializer.Serialize(memStream, request);
                result = memStream.GetBuffer();
            }
            return result;
        }

        public static T DeserializeFromByteArray<T>(byte[] buffer)
        {
            BinaryFormatter deserializer = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream(buffer))
            {
                object newobj = deserializer.Deserialize(memStream);
                return (T)newobj;
            }
        }
    }
}
