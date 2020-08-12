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
		
		public static Vector4 TargetRotorSpeeds(float target_speed, Vector3 destination, Vector3 position,
										    	Vector3 velocity, Vector3 acceleration, float mass, float g,
												Vector3 inertia, Vector3 angular_position, Vector3 angular_velocity,
												float drag_factor, float thrust_factor, float rod_length, float yaw_factor,
												bool degrees=true)
		{
				if (!degrees)
				{
					angular_position.x *= 180.0f / (float)Math.PI;
					angular_position.y *= 180.0f / (float)Math.PI;
					angular_position.z *= 180.0f / (float)Math.PI;
					
					angular_velocity.x *= 180.0f / (float)Math.PI;
					angular_velocity.y *= 180.0f / (float)Math.PI;
					angular_velocity.z *= 180.0f / (float)Math.PI;
				}	


                Vector3 targetVelocity = (destination - position).normalized;
                Debug.Log("targetVelocity BEFORE SPEED x: " + targetVelocity.x + " y: " + targetVelocity.y + " z: " + targetVelocity.z);
				Debug.Log("targetSpeed: " + target_speed);
				targetVelocity *= target_speed;
                Debug.Log("targetVelocity x: " + targetVelocity.x + " y: " + targetVelocity.y + " z: " + targetVelocity.z);
                Vector3 targetAcceleration = targetVelocity - velocity;
                Debug.Log("targetAcceleration x: " + targetAcceleration.x + " y: " + targetAcceleration.y + " z: " + targetAcceleration.z);

				// Cosines and sines cancel out, to give thrust_acceleration
				// TODO: plus/minus!!!!!	
				float thrust_acceleration = Mathf.Sqrt(targetAcceleration.x * targetAcceleration.x
													  + (targetAcceleration.y - g) * (targetAcceleration.y - g)
													  + targetAcceleration.z * targetAcceleration.z);

				Debug.Log("thrust_acceleration: " + thrust_acceleration);
				float thrust = mass * thrust_acceleration;
				Debug.Log("mass: " + mass);	
				Debug.Log("thrust: " + thrust);	
				Vector4 targetTorques;
                targetTorques.w = thrust;

				Vector3 targetAngularPosition = targetAcceleration.normalized;
				// targetAngularPosition.x /= thrust_acceleration;
				// targetAngularPosition.y -= g;
				// targetAngularPosition.y /= thrust_acceleration;
				// targetAngularPosition.z /= thrust_acceleration;
				// targetAngularPosition = targetAngularPosition.normalized;
				Vector3 targetAngularVelocity = targetAngularPosition - angular_position;
				Vector3 targetAngularAcceleration = targetAngularVelocity - angular_velocity;
				// float kX = targetAcceleration.x / thrust_acceleration;
				// float kY = (targetAcceleration.y - g) / thrust_acceleration;
				// float kZ = targetAcceleration.z / thrust_acceleration;
                // Debug.Log("kX: " + kX + " kY: " + kY + " kZ: " + kZ);

				// Find the constants of the quadratic equation
				// float a = 1.0f - kY;
				// float b = kX + kY - 1.0f;
				// float c = (kX + kY + kZ - 1.0f) / 2.0f;
                // Debug.Log("a: " + a + " b: " + b + " c: " + c);

                // from targetAcceleration, find target angular_position to get a target angularAcceleration 
				// double cos_uy = -b + Math.Sqrt(b * b - 4 * a * c) / (2 * a);	
				// double cos_uz = kX + cos_uy + kY - kY * cos_uy;
				// double cos_ux = kY / cos_uz;
                // Debug.Log("cos_ux: " + cos_ux + " cos_uy: " + cos_uy + " cos_uz: " + cos_uz);

				// Target angular position
				// Vector3 targetAngularPosition;
				// targetAngularPosition.x = (float)Math.Acos(cos_ux) * 180.0f / (float)Math.PI;
				// targetAngularPosition.y = (float)Math.Acos(cos_uy) * 180.0f / (float)Math.PI;
				// targetAngularPosition.z = (float)Math.Acos(cos_uz) * 180.0f / (float)Math.PI;
                // Debug.Log("targetAngularPosition x: " + targetAngularPosition.x + " y: " + targetAngularPosition.y + " z: " + targetAngularPosition.z);

				// Vector3 x_acceleration = targetAcceleration.normalized;
				// x_acceleration.x = acceleration.normalized.x;
				// Vector3 y_acceleration = targetAcceleration.normalized;
				// y_acceleration.y = acceleration.normalized.y;
				// Vector3 z_acceleration = targetAcceleration.normalized;
				// z_acceleration.z = acceleration.normalized.z;

				// Vector3 targetAngularVelocity;
				// Vector3 targetAngularAcceleration;// = targetAngularPosition - angular_position;
				// targetAngularVelocity.x = Vector3.SignedAngle(x_acceleration, targetAcceleration.normalized, Vector3.up);
				// targetAngularVelocity.y = Vector3.SignedAngle(y_acceleration, targetAcceleration.normalized, Vector3.up);
				// targetAngularVelocity.z = Vector3.SignedAngle(z_acceleration, targetAcceleration.normalized, Vector3.up);
				// targetAngularAcceleration = targetAngularVelocity - angular_velocity;

                // Debug.Log("targetAngularAcceleration x: " + targetAngularAcceleration.x + " y: " + targetAngularAcceleration.y + " z: " + targetAngularAcceleration.z);
                // Debug.Log("targetAngularVelocity x: " + targetAngularVelocity.x + " y: " + targetAngularVelocity.y + " z: " + targetAngularVelocity.z);

				// float kx = targetAcceleration.x / thrust_acceleration;
				// float ky = (targetAcceleration.y - g) / thrust_acceleration;
				// float kz = targetAcceleration.z / thrust_acceleration;
				// float cos_ux = 0.0f;
				// float cos_uy = 0.0f;
				// float cos_uz = 0.0f;
				// Vector3 error;
				// Vector3 smallest_error = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
				// for (float x = 0.0f; x <= 1.0f; x += 0.02f)
				// {
				// 	for (float y = 0.0f; y <= 1.0f; y += 0.02f)
				// 	{
				// 		for (float z = 0.0f; z <= 1.0f; z += 0.02f)
				// 		{
				// 			error.x = kx - z * Mathf.Sqrt(1.0f - y * y) * Mathf.Sqrt(1.0f - x * x) + y * Mathf.Sqrt(1 - z * z);
				// 			// Take the square of the error	
				// 			error.x *= error.x;	
				// 			error.y = ky - z * x;
				// 			// Take the square of the error	
				// 			error.y *= error.y;
				// 			error.z = kz - Mathf.Sqrt(1 - z * z) * Mathf.Sqrt(1 - y * y) - z * y * Mathf.Sqrt(1 - x * x);
				// 			// Take the square of the error	
				// 			error.z *= error.z;

				// 			if (error.magnitude < smallest_error.magnitude)
				// 			{
				// 				smallest_error = error;
				// 				cos_ux = x;
				// 				cos_uy = y;
				// 				cos_uz = z;
				// 			}

				// 		}
				// 	}
				// }
				// Vector3 targetAngularPosition;
				// targetAngularPosition.x = Mathf.Acos(cos_ux) * 180.0f / Mathf.PI;
				// targetAngularPosition.y = Mathf.Acos(cos_uy) * 180.0f / Mathf.PI;
				// targetAngularPosition.z = Mathf.Acos(cos_uz) * 180.0f / Mathf.PI;
                // Debug.Log("--------- targetAngularPosition x: " + targetAngularPosition.x + " y: " + targetAngularPosition.y + " z: " + targetAngularPosition.z);
				
				// Vector3 targetAngularVelocity = targetAngularPosition - angular_position;
                // Debug.Log("--------- targetAngularVelocity x: " + targetAngularVelocity.x + " y: " + targetAngularVelocity.y + " z: " + targetAngularVelocity.z);
				// Vector3 targetAngularAcceleration = targetAngularVelocity - angular_velocity;
                // Debug.Log("--------- targetAngularAcceleration x: " + targetAngularAcceleration.x + " y: " + targetAngularAcceleration.y + " z: " + targetAngularAcceleration.z);

				float Ixx = inertia.x;
				float Iyy = inertia.y;
				float Izz = inertia.z;
                Debug.Log("++++++++INERTIA++++++++++++ Ixx: " + inertia.x + " Iyy: " + inertia.y + " Izz: " + inertia.z);
				float dux = angular_velocity.x;
				float duy = angular_velocity.y;
				float duz = angular_velocity.z;
				targetTorques.x = targetAngularAcceleration.x * Ixx	- (Iyy - Izz) * dux;
				targetTorques.y = targetAngularAcceleration.y * Iyy	- (Izz - Ixx) * duy;
				targetTorques.z = targetAngularAcceleration.z * Izz	- (Ixx - Iyy) * duz;
				// targetTorques.x = targetAngularAcceleration.x * Ixx	- (Iyy - Izz) * duy * duz;
				// targetTorques.y = targetAngularAcceleration.y * Iyy	- (Izz - Ixx) * duz * dux;
				// targetTorques.z = targetAngularAcceleration.z * Izz	- (Ixx - Iyy) * dux * duy;
                Debug.Log("++++++++TORQUES++++++++++++ w: " + targetTorques.w + " x: " + targetTorques.x + " y: " + targetTorques.y + " z: " + targetTorques.z);

				// Find the constants needed to compute the target rotor speeds
				float W = targetTorques.w / (drag_factor * thrust_factor);
				float X = targetTorques.x / (rod_length * thrust_factor);
				float Y = targetTorques.y / yaw_factor;
				float Z = targetTorques.z / (rod_length * thrust_factor);
                Debug.Log("CONSTANTS W: " + W + " x: " + X + " y: " + Y + " z: " + Z);

				Vector4 targetRotorSpeeds;
				float f1 = (W - Y - 2.0f * Z) / 4.0f;
				float f3 = Z + f1;
				float f2 = (Y - X + f1 + f3) / 2.0f;
				float f4 = X + f2;
				// if (f1 < 0)
				// {
				// 	f2 += f1 / 2.0f;
				// 	f4 += f1 / 2.0f;
				// 	f1 = 0.0f;
				// }
				// if (f2 < 0)
				// {
				// 	f1 += f2 / 2.0f;
				// 	f3 += f2 / 2.0f;
				// 	f2 = 0.0f;
				// }
				// if (f3 < 0)
				// {
				// 	f2 += f3 / 2.0f;
				// 	f4 += f3 / 2.0f;
				// 	f3 = 0.0f;
				// }
				// if (f4 < 0)
				// {
				// 	f1 += f4 / 2.0f;
				// 	f3 += f4 / 2.0f;
				// 	f4 = 0.0f;
				// }
				// float estimatedThrust = drag_factor * thrust_factor * (f1 + f2 + f3 + f4);
				// if (estimatedThrust > thrust)
				// {
				// 	float scaling_factor = thrust / estimatedThrust;
				// 	f1 *= scaling_factor;	
				// 	f2 *= scaling_factor;	
				// 	f3 *= scaling_factor;	
				// 	f4 *= scaling_factor;	
				// }
				// targetRotorSpeeds.w = (float)Math.Sqrt(f1);
				// targetRotorSpeeds.x = (float)Math.Sqrt(f2);
				// targetRotorSpeeds.y = (float)Math.Sqrt(f3);
				// targetRotorSpeeds.z = (float)Math.Sqrt(f4);
				targetRotorSpeeds.w = f1;
				targetRotorSpeeds.x = f2;
				targetRotorSpeeds.y = f3;
				targetRotorSpeeds.z = f4;

				return targetRotorSpeeds;
		}	

		// TODO: documentation
		// Returns the magnitude of the thrust and torques of the quadrotor for the given rotor speeds
		public static Vector4 SpinRotors(Vector4 rotor_speeds, float drag_factor, float thrust_factor, float rod_length, float yaw_factor)
		{
			float f1 = rotor_speeds.w;// * rotor_speeds.w;
			float f2 = rotor_speeds.x;// * rotor_speeds.x;
			float f3 = rotor_speeds.y;// * rotor_speeds.y;
			float f4 = rotor_speeds.z;// * rotor_speeds.z;

			Vector4 torques;
			torques.w = drag_factor * thrust_factor * (f1 + f2 + f3 + f4); // thrust
			torques.x = rod_length  * thrust_factor * (f4 - f2);           // x_torque
			torques.y = yaw_factor  * (f2 + f4 - f1 - f3);                 // y_torque
			torques.z = rod_length  * thrust_factor * (f3 - f1);           // z_torque

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

			// if (degrees)
			// {
			// 	ux *= (float)Math.PI / 180.0f;
			// 	uy *= (float)Math.PI / 180.0f;
			// 	uz *= (float)Math.PI / 180.0f;
			// }

			float sin_ux = (float)Math.Sin(ux);
			float cos_ux = (float)Math.Cos(ux);
			float sin_uy = (float)Math.Sin(uy);
			float cos_uy = (float)Math.Cos(uy);
			float sin_uz = (float)Math.Sin(uz);
			float cos_uz = (float)Math.Cos(uz);

			float thrust_acceleration = thrust / mass;

			Vector3 acceleration;
			acceleration.x =     thrust_acceleration * ux;//* (cos_uz * sin_uy * sin_ux - cos_uy * sin_uz);
			acceleration.y = g + thrust_acceleration * uy;//* (cos_uz * cos_ux);
			acceleration.z =     thrust_acceleration * uz;//* (sin_uz * sin_uy + cos_uz * cos_uy * sin_ux);

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

			// if (!degrees)
			// {
			// 	dux *= 180.0f / (float)Math.PI;
			// 	duy *= 180.0f / (float)Math.PI;
			// 	duz *= 180.0f / (float)Math.PI;
			// }

			Vector3 angularAcceleration;
			angularAcceleration.x = ((Iyy - Izz) * dux + torques.x) / Ixx;
			angularAcceleration.y = ((Izz - Ixx) * duy + torques.y) / Iyy;
			angularAcceleration.z = ((Ixx - Iyy) * duz + torques.z) / Izz;
			// angularAcceleration.x = ((Iyy - Izz) * duy * duz + torques.x) / Ixx;
			// angularAcceleration.y = ((Izz - Ixx) * duz * dux + torques.y) / Iyy;
			// angularAcceleration.z = ((Ixx - Iyy) * dux * duy + torques.z) / Izz;

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