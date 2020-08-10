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
			// Vector3 newPosition = Rotation(this.transform.localPosition, new Vector3(45, 45, 45));
            // this.transform.localPosition = newPosition;
        }
	}

	// TODO: documentation
	// inputs:
	// -> frame = velocity_B (velocities u, v, w in the body frame)
	// -> rotation = angularPosition_I (phi, theta, psi)
	// returns the velocity_I the velocity in the inertial frame
	private Vector3 Rotation(Vector3 frame, Vector3 rotation, bool degrees=true)
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

		Vector3 R1 = new Vector3(suz * sux * suy + cuz * cuy,      cuz * sux * suy - suz * cuy,      cux * suy);
		Vector3 R2 = new Vector3(suz * cux,                        cuz * cux,                        -sux);
		Vector3 R3 = new Vector3(suz * sux * cuy - cuz * suy,      cuz * sux * cuy + suz * suy,      cux * cuy);

		Vector3 rotatedFrame = frame;	
		rotatedFrame.x = Vector3.Dot(R1, frame);
		rotatedFrame.y = Vector3.Dot(R2, frame);
		rotatedFrame.z = Vector3.Dot(R3, frame);

		return rotatedFrame;
	}

	// TODO: documentation
	// phi dot, theta dot, psi dot
	// inputs:
	// -> direction = angular_velocity_B (angular_velocities p, r, q in the body frame)
	// -> rotation = angularPosition_I (phi, theta, psi)
	// return w = w_I = angularVelocity_I (in the inertial frame)	
	private Vector3 InverseJacobian(Vector3 direction, Vector3 rotation, bool degrees=true)
	{
		float p = direction.x;
		float r = direction.y;
		float q = direction.z;

		float ux = rotation.x;
		float uz = rotation.z;

		if (degrees)
		{
			ux *= (float)Math.PI / 180.0f;
			uz *= (float)Math.PI / 180.0f;
		}

		float cos_ux = (float)Math.Cos(ux);
		float tan_ux = (float)Math.Tan(ux);
		float sin_uz = (float)Math.Sin(uz);
		float cos_uz = (float)Math.Cos(uz);

		float dx = r * cos_uz - p * sin_uz;
		float dy = p * (cos_uz / cos_ux) + r * (sin_uz / cos_uz);
		float dz = q + p * cos_uz * tan_ux + r * sin_uz * tan_ux;

		return new Vector3(dx, dy, dz);
	}
    
    // Returns the magnitude of the thrust and torques of the quadrotor for the given rotor speeds
    private Vector4 SpinRotors(Vector4 rotor_speeds, float drag_factor, float thrust_factor, float rod_length, float yaw_factor)
    {
        float f1 = rotor_speeds.w * rotor_speeds.w;
        float f2 = rotor_speeds.x * rotor_speeds.x;
        float f3 = rotor_speeds.y * rotor_speeds.y;
        float f4 = rotor_speeds.z * rotor_speeds.z;

        Vector4 thrust_forces;
        thrust_forces.w = drag_factor * thrust_factor * (f1 + f2 + f3 + f4); // thrust
        thrust_forces.x = rod_length  * thrust_factor * (f3 - f2);           // x_torque
        thrust_forces.y = yaw_factor  * (f1 - f2 - f3 + f4);                 // y_torque
        thrust_forces.z = rod_length  * thrust_factor * (f1 - f4);           // z_torque

        return thrust_forces;
    }

    // Returns the acceleration in the inertial frame
    // **x, **y, **z
    public Vector3 Acceleration(float thrust, float mass, float g, Vector3 angular_position, bool degrees=true)
    {
		float ux = angular_position.x;
		float uy = angular_position.y;
		float uz = angular_position.z;

		if (degrees)
		{
			ux *= (float)Math.PI / 180.0f;
			uy *= (float)Math.PI / 180.0f;
			uz *= (float)Math.PI / 180.0f;
		}

		float sin_ux = (float)Math.Sin(ux);
		float cos_ux = (float)Math.Cos(ux);
		float sin_uy = (float)Math.Sin(uy);
		float cos_uy = (float)Math.Cos(uy);
		float sin_uz = (float)Math.Sin(uz);
		float cos_uz = (float)Math.Cos(uz);

		float thrust_acceleration = thrust / mass;

		Vector3 acceleration;
		acceleration.x =   - thrust_acceleration * (cos_uz * sin_uy * sin_ux - cos_uy * sin_uz);
		acceleration.y = g - thrust_acceleration * (cos_uz * cos_ux);
		acceleration.z =   - thrust_acceleration * (sin_uz * sin_uy + cos_uz * cos_uy * sin_ux);

		return acceleration;
    }    

	public Vector3 AccelerationBody(float thrust, float mass, float g, Vector3 wind_disturbance, Vector3 velocity_body, Vector3 angular_velocity_body, Vector3 angular_position, bool degrees=true)
	{
		float vx_b = velocity_body.x;
		float vy_b = velocity_body.y;
		float vz_b = velocity_body.z;

		float uvx_b = angular_velocity_body.x;
		float uvy_b = angular_velocity_body.y;
		float uvz_b = angular_velocity_body.z;

		if (!degrees)
		{
			uvx_b *= 180.0f / (float)Math.PI;
			uvy_b *= 180.0f / (float)Math.PI;
			uvz_b *= 180.0f / (float)Math.PI;
		}

		float ux = angular_position.x;
		float uz = angular_position.z;

		if (degrees)
		{
			ux *= (float)Math.PI / 180.0f;
			uz *= (float)Math.PI / 180.0f;
		}

		float sin_ux = (float)Math.Sin(ux);
		float cos_ux = (float)Math.Cos(ux);
		float sin_uz = (float)Math.Sin(uz);
		float cos_uz = (float)Math.Cos(uz);


		Vector3 acceleration_body;
		acceleration_body.x = uvz_b * vy_b - uvy_b * vz_b + g * sin_uz * cos_ux + wind_disturbance.x / mass;
		acceleration_body.y = uvx_b * vz_b - uvz_b * vx_b + g * cos_ux * cos_uz + (wind_disturbance.y - thrust) / mass;
		acceleration_body.z = uvy_b * vx_b - uvx_b * vy_b - g * sin_ux          + wind_disturbance.z / mass;

		return acceleration_body;
	}

    // Returns the angular acceleration in the inertial frame
    // **phi, **theta, **psi
    public Vector3 AngularAcceleration(Vector4 thrust_forces, Vector3 inertia, Vector3 angular_velocity, bool degrees=true)
    {

		float Ixx = inertia.x;
		float Iyy = inertia.y;
		float Izz = inertia.z;

		float dux = angular_velocity.x;
		float duy = angular_velocity.y;
		float duz = angular_velocity.z;

		if (!degrees)
		{
			dux *= 180.0f / (float)Math.PI;
			duy *= 180.0f / (float)Math.PI;
			duz *= 180.0f / (float)Math.PI;
		}

		Vector3 angularAcceleration;
		angularAcceleration.x = ((Iyy - Izz) * duy * duz + forces.x) / Ixx;
		angularAcceleration.y = ((Izz - Ixx) * duz * dux + forces.y) / Iyy;
		angularAcceleration.z = ((Ixx - Iyy) * dux * duy + forces.z) / Izz;

		return angularAcceleration;	
    }    


	public Vector3 AngularAccelerationBody(Vector3 inertia, Vector3 angular_wind_disturbance)
	{

	}

}
