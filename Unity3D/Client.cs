/**	Kinect Client Class
		Description:  This script handles interactions w/ a Kinect server currently feeding joint data to any connected clients via TCP.
		Notes: The TcpServer was already written when I started this little project but I really should have written a UDP server.
		
		@author Jeremy Carson
		@website http://www.seethroughskin.com/blog/?p=1159
		@requirements
			KinectDaemon(bundled w/ script) - KinectDaemon is simple server that feeds joint data via TCP packets
			KinectSerializables(bundled w/ script)
			Microsoft KinectSDK - Project built against the first beta drop, can't recall the version.
		
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
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Client : MonoBehaviour {
	public Transform LeftElbow = null;
	public Transform LeftHand = null;
	public Transform LeftShoulder = null;
	
	public Transform RightElbow = null;
	public Transform RightHand = null;
	public Transform RightShoulder = null;
	
	public Transform LeftLeg = null;
	public Transform LeftKnee = null;
	public Transform LeftFoot = null;
	
	public Transform RightLeg = null;
	public Transform RightKnee = null;
	public Transform RightFoot = null;
	
	public Transform DebugSphere1;
	public Transform DebugSphere2;
	public GameObject JointPrefab = null;
	public Transform JointMassCenter = null;
	
	public Matrix4x4 MirrorMatrix = Matrix4x4.identity;
	
	public bool CenterData = false;
	public int Port = 3000;
	public string IpAddr= "127.0.0.1";

	private TcpClient _tcpClient = new TcpClient();
	private NetworkStream _clientStream = null;
	private bool _isShuttingDown = false;

	private float _elapsedTime = 0.0f;
	
	//TCP request rate
	public float SampleRate = 0.15f;

	//Min/Max Range values (replace w/ actual Kinect bounding ranges)
	public Vector3	Min = Vector3.zero;
	public Vector3	Max = Vector3.zero;

	//Internal lookup table for joint locations.
	private Dictionary<string,GameObject> Points = new Dictionary<string,GameObject>();

	public Client(){

	}
	
	//Kinect client to server.  Server much already be on.
	public void Connect(){
		_isShuttingDown = false;

		IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(IpAddr), Port);
		_tcpClient.Connect(serverEndPoint);
		_clientStream = _tcpClient.GetStream();
	}
	
	//Disconnect client from server.
	public void Disconnect()
	{
		if(_tcpClient==null)return;
		
		_isShuttingDown = true;
		_tcpClient.Close();
	}
	
	//Send text message to server (Server doesn't currently have any internal parser, all values sent to server return joint data.)
	public void SendMessageToServer(string msg)
	{
		if(_clientStream==null)return;
		
		ASCIIEncoding encoder = new ASCIIEncoding();
		byte[] bbuffer = encoder.GetBytes(msg);

		_clientStream.Write(bbuffer, 0, bbuffer.Length);
		_clientStream.Flush();
	}
	
	//Request joint data from server (Server returns joint data for any input string but eventually this would have a unique bit flag)
	public void RequestJointUpdate(){
		SendMessageToServer("j");
	}
	
	//Deserialize packet structure
	public static T DeserializeFromByteArray<T>(byte[] buffer)
	{
		BinaryFormatter deserializer = new BinaryFormatter();
		using (MemoryStream memStream = new MemoryStream(buffer))
		{
			object newobj = deserializer.Deserialize(memStream);
			return (T)newobj;
		}
	}

	// Use this for initialization
	void Start () {
		Connect();
	}
	
	void OnDestroy(){
		Disconnect();
	}
	
	void CenterModel(){
		if(!CenterData)return;
		Vector3 offset = Vector3.zero;
		if(JointMassCenter!=null)offset = JointMassCenter.position;
		MirrorMatrix[2,2] = -1;
		
		try {
			Vector3 center = Vector3.zero;
			center = MirrorMatrix *Points["HipCenter"].transform.position;
			foreach (KeyValuePair<string,GameObject> kvp in Points){
			
					kvp.Value.transform.position = (Vector3)(MirrorMatrix * kvp.Value.transform.position) - center + offset ;
				
			}
		}catch{
		}
	}
	Quaternion GetRotationTo(Vector3 start, Vector3 dest, Vector3 fallback){
		Quaternion q;
		Vector3 v0 = start;
		Vector3 v1 = dest;

		v0.Normalize();
		v1.Normalize();

		float d = Vector3.Dot(v0,v1);
		if(d>=1.0f)return Quaternion.identity;

		if(d < (1e-6f - 1.0f)){
			if(fallback != Vector3.zero){
				q  =  Quaternion.AngleAxis(Mathf.PI, fallback);
			} else {
				Vector3 axis = Vector3.Cross(Vector3.right, start);
				if(axis.magnitude <= Mathf.Epsilon) axis = Vector3.Cross(Vector3.up, start);
				axis.Normalize();
				q =  Quaternion.AngleAxis(Mathf.PI, axis);
			}
		} else {
			float s = Mathf.Sqrt((1+d)*2);
			float invs = 1/s;
			Vector3 c = Vector3.Cross(v0,v1);

			q.x = c.x * invs;
			q.y = c.y * invs;
			q.z = c.z * invs;
			q.w = s * 0.5f;
			//q.Normalize();
		}
		return q;
	}
	float AngleBetween(Vector3 from, Vector3 dest)  {
		float len = from.magnitude *  dest.magnitude;
		if(len < Mathf.Epsilon)	len = Mathf.Epsilon;

		float f = Vector3.Dot(from,dest) / len;
		if(f>1.0f)f=1.0f;
		else if ( f < -1.0f) f = -1.0f;

		return Mathf.Acos(f) * 180.0f / (float)Math.PI;
	}
	void TestArm(){
		try {
			Vector3 offset = Vector3.zero;
			if(JointMassCenter!=null)offset = JointMassCenter.position;
			
			Vector3 hand = Points["HandLeft"].transform.position;
			Vector3 elbow = Points["ElbowLeft"].transform.position;
			Vector3 shoulder = Points["ShoulderLeft"].transform.position;
			Vector3 shoulderCenter = Points["ShoulderCenter"].transform.position;
			
			Vector3 bicep = elbow - shoulder;
			Vector3 forearm = hand - elbow;
			Vector3 upperarm = shoulder - elbow;
			
			Quaternion lastRotation;
			
			Quaternion o = Quaternion.identity;
			{	//Shoulder, yaw
				Vector3 tmp = upperarm;
				tmp.y = 0;
				tmp.Normalize();
				o =  GetRotationTo(Vector3.right,tmp,Vector3.zero);
				
				if(LeftShoulder!=null)Debug.DrawRay (LeftShoulder.transform.position,tmp * 5, Color.green);
			}
			
			{	//Shoulder, pitch
				Vector3 tmp = upperarm;
				tmp = Quaternion.Inverse(o) * upperarm;
				tmp.Normalize();
				o = o * GetRotationTo(Vector3.right,tmp, Vector3.zero);
				
				if(LeftShoulder!=null)Debug.DrawRay (LeftShoulder.transform.position,tmp * 5, Color.red);
			}
			if(false)
			{	//Shoulder, roll
				Vector3 tmp =  Quaternion.Inverse(o)  * forearm;
			
				tmp.Normalize();
				o = GetRotationTo(Vector3.forward,tmp, Vector3.zero) * o;
			//	if(LeftElbow!=null)Debug.DrawRay (LeftElbow.transform.position,tmp * 5, Color.blue);
			}
			if(LeftShoulder!=null)LeftShoulder.localRotation = o;
			
			{	//Elbow, pitch
				Vector3 tmp =  Quaternion.Inverse(o) * forearm;
				tmp.Normalize();
				o = GetRotationTo(Vector3.left,tmp, Vector3.zero);
				if(LeftElbow!=null)Debug.DrawRay (LeftElbow.transform.position,tmp * 5, Color.blue);
			}
			if(LeftElbow!=null)LeftElbow.localRotation = o;
			
			//========= Right arm =============
			hand = Points["HandRight"].transform.position;
			elbow = Points["ElbowRight"].transform.position;
			shoulder = Points["ShoulderRight"].transform.position;
			shoulderCenter = Points["ShoulderCenter"].transform.position;
			
			bicep = elbow - shoulder;
			forearm = hand - elbow;
			upperarm = shoulder - elbow;
			{	//Shoulder, yaw
				Vector3 tmp = upperarm;
				tmp.y = 0;
				tmp.Normalize();
				o =  GetRotationTo(Vector3.left,tmp,Vector3.zero);
				
				if(RightShoulder!=null)Debug.DrawRay (RightShoulder.transform.position,tmp * 5, Color.green);
			}
		
			{	//Shoulder, pitch
				Vector3 tmp = upperarm;
				tmp = Quaternion.Inverse(o) * upperarm;
				tmp.Normalize();
				o =GetRotationTo(Vector3.left,tmp, Vector3.zero) * o;
				
				if(RightShoulder!=null)Debug.DrawRay (RightShoulder.transform.position,tmp * 5, Color.red);
			}
			if(false)
			{	//Shoulder, roll
				Vector3 tmp =  Quaternion.Inverse(o)  * forearm;
			
				tmp.Normalize();
				o = o * GetRotationTo(Vector3.forward,tmp, Vector3.zero);
			//	if(RightShoulder!=null)Debug.DrawRay (RightShoulder.transform.position,tmp * 5, Color.blue);
			}
			
			if(RightShoulder!=null)RightShoulder.localRotation = o;
		
			{	//Elbow, pitch
				Vector3 tmp =  Quaternion.Inverse(o) * forearm;
				tmp.Normalize();
				o = GetRotationTo(Vector3.right,tmp, Vector3.zero);
				if(RightElbow!=null)Debug.DrawRay (RightElbow.transform.position,tmp * 5, Color.blue);
			}

			if(RightElbow!=null)RightElbow.localRotation = o;
		}catch{
		}
	}
	// Update is called once per frame
	void Update () {
		
		TestArm();
		if(_clientStream==null)return;
		{	//Handle sampling threshold
			_elapsedTime += Time.deltaTime;
			if(_elapsedTime < SampleRate)return;
			_elapsedTime = 0.0f;
		}
		
		//Out simple system has zero back end protection to packets are only sent by request.  W/O this request the TCP stack fills and the problem lags, badly.
		RequestJointUpdate();
		
		//Make sure to parse all packets in client stream or they will build up.
		while (_clientStream.DataAvailable)
		{
			try
			{
				byte[] message = new byte[4096];
				int bytesRead = _clientStream.Read(message, 0, 4096);
				KinectPacket packet = DeserializeFromByteArray<KinectPacket>(message);
				
				foreach (KeyValuePair<string, KinectPoint> kvp in packet.Messages){
					//Debug.Log(kvp.Key + "found");
					string [] tokens = kvp.Value.ToString().Split(' ');
					string key = kvp.Key;
					if(tokens.Length == 3){
						//Debug.Log(kvp.Key + " " + kvp.Value.ToString());
						GameObject go = null;
						if(Points.ContainsKey(key)){
							go = Points[key];
						
						} else {
							go = (GameObject)Instantiate(JointPrefab,Vector3.zero,Quaternion.identity);
							go.name = key;
							Points[key] = go;
						}
						
						//Normalize data to [0...1] on all axes.  Ranges are specified manually but they should be taken from the Kinect specs which I didn't have handy.
						Vector3 pos = new Vector3(	float.Parse(tokens[0]),float.Parse(tokens[1]),float.Parse(tokens[2]));
						pos.x = (pos.x - Min.x) / (Max.x - Min.x);
						pos.y = (pos.y - Min.y) / (Max.y - Min.y);
						pos.z = (pos.z - Min.z) / (Max.z - Min.z);

					
						if(go)
							go.transform.position = pos;
					}
				}
				CenterModel();
				//TestArm();
			}
			catch(SystemException ex)
			{
				Debug.Log("Problem w/ client: " + ex.Message + "," + ex.StackTrace);
			}
		}
	}
}
