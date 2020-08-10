namespace ISAACS
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public static class QuadrotorDynamics
	{

		// TODO: documentation
		// inputs:
		// -> frame = velocity_B (velocities u, v, w in the body frame)
		// -> rotation = angularPosition_I (phi, theta, psi)
		// returns the velocity_I the velocity in the inertial frame
		public static Vector3 Rotation(Vector3 frame, Vector3 rotation, bool degrees=true)
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
		public static Vector3 InverseJacobian(Vector3 direction, Vector3 rotation, bool degrees=true)
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
		
		// TODO: documentation
		// Returns the magnitude of the thrust and torques of the quadrotor for the given rotor speeds
		public static Vector4 SpinRotors(Vector4 rotor_speeds, float drag_factor, float thrust_factor, float rod_length, float yaw_factor)
		{
			float f1 = rotor_speeds.w * rotor_speeds.w;
			float f2 = rotor_speeds.x * rotor_speeds.x;
			float f3 = rotor_speeds.y * rotor_speeds.y;
			float f4 = rotor_speeds.z * rotor_speeds.z;

			Vector4 torques;
			torques.w = drag_factor * thrust_factor * (f1 + f2 + f3 + f4); // thrust
			torques.x = rod_length  * thrust_factor * (f3 - f2);           // x_torque
			torques.y = yaw_factor  * (f1 - f2 - f3 + f4);                 // y_torque
			torques.z = rod_length  * thrust_factor * (f1 - f4);           // z_torque

			return torques;
		}

		// TODO: documentation
		// Returns the acceleration in the inertial frame
		// **x, **y, **z
		public static Vector3 Acceleration(float thrust, float mass, float g, Vector3 angular_position, bool degrees=true)
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
			acceleration.x =   + thrust_acceleration * (cos_uz * sin_uy * sin_ux - cos_uy * sin_uz);
			acceleration.y = g + thrust_acceleration * (cos_uz * cos_ux);
			acceleration.z =   + thrust_acceleration * (sin_uz * sin_uy + cos_uz * cos_uy * sin_ux);

			return acceleration;
		}    

		// TODO: documentation
		// Returns the angular acceleration in the inertial frame
		// **phi, **theta, **psi
		public static Vector3 AngularAcceleration(Vector4 torques, Vector3 inertia, Vector3 angular_velocity, bool degrees=true)
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
			angularAcceleration.x = ((Iyy - Izz) * duy * duz + torques.x) / Ixx;
			angularAcceleration.y = ((Izz - Ixx) * duz * dux + torques.y) / Iyy;
			angularAcceleration.z = ((Ixx - Iyy) * dux * duy + torques.z) / Izz;

			return angularAcceleration;	
		}    

		// TODO: documentation
		public static Vector3 AccelerationBody(float thrust, float mass, float g, Vector3 wind_disturbance, Vector3 velocity_body, Vector3 angular_velocity_body, Vector3 angular_position, bool degrees=true)
		{
			float dx_b = velocity_body.x;
			float dy_b = velocity_body.y;
			float dz_b = velocity_body.z;

			float dux_b = angular_velocity_body.x;
			float duy_b = angular_velocity_body.y;
			float duz_b = angular_velocity_body.z;

			if (!degrees)
			{
				dux_b *= 180.0f / (float)Math.PI;
				duy_b *= 180.0f / (float)Math.PI;
				duz_b *= 180.0f / (float)Math.PI;
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
			acceleration_body.x = duz_b * dy_b - duy_b * dz_b + g * sin_uz * cos_ux + wind_disturbance.x / mass;
			acceleration_body.y = dux_b * dz_b - duz_b * dx_b + g * cos_ux * cos_uz + (wind_disturbance.y - thrust) / mass;
			acceleration_body.z = duy_b * dx_b - dux_b * dy_b - g * sin_ux          + wind_disturbance.z / mass;

			return acceleration_body;
		}

		// TODO: documentation
		public static Vector3 AngularAccelerationBody(Vector4 torques, Vector3 inertia, Vector3 angular_wind_disturbance, Vector3 angular_velocity_body, bool degrees=true)
		{
			float torque_x = torques.x;
			float torque_y = torques.y;
			float torque_z = torques.z;

			float Ixx = inertia.x;
			float Iyy = inertia.y;
			float Izz = inertia.z;

			float dux_b = angular_velocity_body.x;
			float duy_b = angular_velocity_body.y;
			float duz_b = angular_velocity_body.z;

			float wx = angular_wind_disturbance.x;
			float wy = angular_wind_disturbance.y;
			float wz = angular_wind_disturbance.z;

			if (!degrees)
			{
				dux_b *= 180.0f / (float)Math.PI;
				duy_b *= 180.0f / (float)Math.PI;
				duz_b *= 180.0f / (float)Math.PI;

				wx *= 180.0f / (float)Math.PI;
				wy *= 180.0f / (float)Math.PI;
				wz *= 180.0f / (float)Math.PI;
			}

			Vector3 angular_acceleration_body;
			angular_acceleration_body.x = ((Iyy - Izz) * duy_b * duz_b + torque_x + wx) / Ixx;
			angular_acceleration_body.y = ((Izz - Ixx) * dux_b * duz_b + torque_y + wy) / Iyy;
			angular_acceleration_body.z = ((Ixx - Iyy) * dux_b * duy_b + torque_z + wz) / Izz;

			return angular_acceleration_body;
		}

	}
}