using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSimulationTransforms : MonoBehaviour {

	// For test purposes only
	void Update()
	{
        if (Input.GetKeyUp("z"))
		{
            this.transform.localPosition = new Vector3(2.44f, 5.61f, 4.22f);
		}
        if (Input.GetKeyUp("x"))
        {
			Vector3 newPosition = R(this.transform.localPosition, new Vector3(45, 45, 45));
			// Vector3 newPosition = Rx(this.transform.position, 45);
			// Vector3 newPosition = Ry(this.transform.position, 45);
			// Vector3 newPosition = Rz(this.transform.position, 45);
            this.transform.localPosition = newPosition;
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

		float sin_theta = (float)Math.Sin(theta);
		float cos_theta = (float)Math.Cos(theta);

		float y = frame.y;
		float z = frame.z;

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

		float sin_theta = (float)Math.Sin(theta);
		float cos_theta = (float)Math.Cos(theta);

		float x = frame.x;
		float z = frame.z;
		/*
		|  cθ -sθ  0  || z |
		|  sθ  cθ  0  || x |
		|  0   0   1  || y |
		*/	
		frame.x =  z * sin_theta + x * cos_theta;
		frame.z =  z * cos_theta - x * sin_theta;
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

		float sin_theta = (float)Math.Sin(theta);
		float cos_theta = (float)Math.Cos(theta);

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

		float sux = (float)Math.Sin(ux);
		float cux = (float)Math.Cos(ux);
		float suy = (float)Math.Sin(uy);
		float cuy = (float)Math.Cos(uy);
		float suz = (float)Math.Sin(uz);
		float cuz = (float)Math.Cos(uz);

		Vector3 rotatedFrame = frame;	

		Vector3 R1 = new Vector3(suz * sux * suy + cuz * cuy,      cuz * sux * suy - suz * cuy,      cux * suy);
		Vector3 R2 = new Vector3(suz * cux,                        cuz * cux,                        -sux);
		Vector3 R3 = new Vector3(suz * sux * cuy - cuz * suy,      cuz * sux * cuy + suz * suy,      cux * cuy);

		rotatedFrame.x = Vector3.Dot(R1, frame);
		rotatedFrame.y = Vector3.Dot(R2, frame);
		rotatedFrame.z = Vector3.Dot(R3, frame);

		return rotatedFrame;
	}


	// TODO: documentation
	public Vector3 T(Vector3 angularVelocity_B, Vector3 angularPosition_I, bool degrees=true)
	{
		float p = angularVelocity_B.x;
		float r = angularVelocity_B.y;
		float q = angularVelocity_B.z;

		float X = angularPosition_I.x;
		float Y = angularPosition_I.y;
		float Z = angularPosition_I.z;

		if (degrees)
		{
			X *= (float)Math.PI / 180.0f;
			Y *= (float)Math.PI / 180.0f;
			Z *= (float)Math.PI / 180.0f;
		}

		cosX = (float)Math.Cos(X);
		tanX = (float)Math.Sin(X);
		cosZ = (float)Math.Cos(Z);
		sinZ = (float)Math.Sin(Z);

		float dX = r * cosZ - p * sinZ;
		float dY = p * (cosZ / cosX) + r * (sinZ / cosZ);
		float dZ = q + p * cosZ * tanX + r * sinZ * tanX;

		return new Vector3(dX, dY, dZ);
	}	

}
