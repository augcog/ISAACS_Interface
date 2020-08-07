using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSimulationTransforms : MonoBehaviour {

	// For test purposes only
	void Update()
	{
        if (Input.GetKeyUp("x"))
        {
			// Vector3 rotationDelta = R(this.transform.localPosition, new Vector3(0, 45, 0));
			Vector3 rotationDelta = Rz(this.transform.position, 90);
			Debug.Log("**************************************");
            this.transform.position = rotationDelta;
        }
	}


	// TODO: documentation
	// Rotate the frame by an angle theta along the x-axis.
	public Vector3 Rx(Vector3 frame, float theta, bool degrees=true)
	{
		// If theta was given in degrees, convert it to radians.	
		if (degrees)
		{
			theta *= (float)Math.PI / 180.0f;
		}
		float cos_theta = (float)Math.Cos(theta);
		float sin_theta = (float)Math.Sin(theta);
		float z = frame.z;
		float y = frame.y;

		/*
		|  cθ  0   sθ || z |
		|  0   1   0  || x |
		| -sθ  0   cθ || y |
		*/	
		frame.z =  z * cos_theta + y * sin_theta;
		frame.y = -z * sin_theta + y * cos_theta;
		return frame;
	}

	// TODO: documentation
	// Rotate the frame by an angle theta along the y-axis.
	public Vector3 Ry(Vector3 frame, float theta, bool degrees=true)
	{
		// If theta was given in degrees, convert it to radians.	
		if (degrees)
		{
			theta *= (float)Math.PI / 180.0f;
		}
		float cos_theta = (float)Math.Cos(theta);
		float sin_theta = (float)Math.Sin(theta);
		float z = frame.z;
		float x = frame.x;
		/*
		|  cθ -sθ  0  || z |
		|  sθ  cθ  0  || x |
		|  0   0   1  || y |
		*/	
		frame.z =  z * cos_theta - x * sin_theta;
		frame.x =  z * sin_theta + x * cos_theta;
		return frame;
	}

	// TODO: documentation
	// Rotate the frame by an angle theta along the z-axis.
	public Vector3 Rz(Vector3 frame, float theta, bool degrees=true)
	{
		// If theta was given in degrees, convert it to radians.	
		if (degrees)
		{
			theta *= (float)Math.PI / 180.0f;
		}
		float cos_theta = (float)Math.Cos(theta);
		float sin_theta = (float)Math.Sin(theta);
		float x = frame.x;
		float y = frame.y;
		/*
		|  1  0   0  || z |
		|  0  cθ -sθ || x |
		|  0  sθ  cθ || y |
		*/	
		frame.x =  x * cos_theta - y * sin_theta;
		frame.y =  x * sin_theta + y * cos_theta;
		return frame;
	}


	// TODO: documentation
	public Vector3 R(Vector3 frame, Vector3 rotation, bool degrees=true)
	{
		float ux = rotation.x;
		float uy = rotation.y;
		float uz = rotation.z;

		// If theta was given in degrees, convert it to radians.	
		if (degrees)
		{
			ux *= (float)Math.PI / 180.0f;
			uy *= (float)Math.PI / 180.0f;
			uz *= (float)Math.PI / 180.0f;
		}

		float cux = (float)Math.Cos(ux);
		float sux = (float)Math.Sin(ux);
		float cuy = (float)Math.Cos(uy);
		float suy = (float)Math.Sin(uy);
		float cuz = (float)Math.Cos(uz);
		float suz = (float)Math.Sin(uz);

		Vector3 rotatedFrame = frame;	

		Vector3 R1 = new Vector3(cux * cuy, suz * sux * cuy - cuz * suy, cuz * sux * cuy + suz * suy);
		Vector3 R2 = new Vector3(cux * suy, suz * sux * suy + cuz * cuy, cuz * sux * suy - suz * cuy);
		Vector3 R3 = new Vector3(-sux,      suz * cux,                   cuz * cux);

		rotatedFrame.z = Vector3.Dot(R1, frame);
		rotatedFrame.x = Vector3.Dot(R2, frame);
		rotatedFrame.y = Vector3.Dot(R3, frame);

		return rotatedFrame;
	}

}
