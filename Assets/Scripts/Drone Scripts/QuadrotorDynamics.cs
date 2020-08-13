namespace ISAACS
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public static class QuadrotorDynamics
	{

		// The free fall acceleration on the Earth's surface.
		private static Vector3 g = new Vector3(0.0f, -9.81f, 0.0f);




		/*
				f1
				O-->	
			  /   \
		  f4 O-->  O--> f2     
			  \   /
				O-->
				f3	

		x-axis: f2 to f4
		y-axis: out of the page
		z-axis: f1 to f3
		*/	

		/// <summary>
        /// Given the individual thrust of each rotor ("rotor forces"), this method computes the quadrotor's total thrust and torques.
		/// For simplicity, only the sign and magnitude of each rotor force is required, and only the sign and magnitude of the total thrust and torques is returned.
        /// </summary>
        /// <param name="rotorForces">The signed magnitude of the force exterted by each rotor, in the order shown in the diagram above.</param>
        /// <param name="dragFactor">A damping factor for the thrust, representing resistance by the drag.</param>
        /// <param name="thrustFactor">The contribution of each rotor force to the total thrust, as well as its x-torque ("roll force") and z-torque ("pitch force").</param>
        /// <param name="rodLength">The distance between two opposing rotors, such as O(f1) and O(f3). It is assumed that O(f1)->O(f3) and O(f2)->O(f4) are equal.</param>
        /// <param name="yawFactor">The contribution of each rotor force to the y-torque ("yaw force").</param>
        /// <returns>The signed mangitudes of the quadrotor's total thrust and torques, for the given rotor forces.</returns>
		public static Vector4 SpinRotors(Vector4 rotorForces, float dragFactor, float thrustFactor, float rodLength, float yawFactor)
		{	
			float f1 = rotorForces.w;
			float f2 = rotorForces.x;
			float f3 = rotorForces.y;
			float f4 = rotorForces.z;

			Vector4 torques;
			torques.w = dragFactor * thrustFactor * (f1 + f2 + f3 + f4); // Thrust from all four rotors, accelerating the quadrotor in the direction of its y-axis.
			torques.x = rodLength  * thrustFactor * (f3 - f1);           // Torque rotating the quadrotor around its x-axis ("roll force").
			torques.y = yawFactor  * (f2 + f4 - f1 - f3);                // Torque rotating the quadrotor around its y-axis ("yaw force").
			torques.z = rodLength  * thrustFactor * (f4 - f2);           // Torque rotating the quadrotor around its z-axis ("pitch force").

			return torques;
		}




		/// <summary>
        /// Given the quadrotor's thrust and mass, this method computes its acceleration in the inertial frame.
		/// Since Unity transforms refer to the inertial frame, the result can be directly integrated to solve for velocity and position.
        /// </summary>
        /// <param name="thrust">The signed magnitude of the total thrust exerted by the quadrotor.</param>
        /// <param name="mass">The total mass of the quadrotor.</param>
        /// <param name="direction">The direction of acceleration. Can either be the coordinates of point in the desired direction, or a set of angles specifying the quadrotor's angular position in the inertial frame.</param>
        /// <param name="gravitationalAcceleration">The acceleration due to gravity. Defaults to (0, -9.81, 0), as on the Earth's surface.</param>
        /// <param name="angles">Whether the direction vector holds angles, rather than the coordinates of a desired direction.</param>
        /// <param name="degrees">Whether the angles of the direction vector are in degrees, or radians.</param>
        /// <returns>The acceleration of the quadrotor in the inertial frame.</returns>
		public static Vector3 Acceleration(float thrust, float mass, Vector3 direction, Vector3 gravitationalAcceleration=g, bool angles=false, bool degrees=true)
		{
			Vector3 acceleration;
			// From Newton's law (F = ma), the acceleration of an object from an external force is equal to the force divided by the object's mass (a = F/m).
			float thrustAccelerationMagnitude = thrust / mass;

			// Check if the direction vector holds coordinates, rather than angles.
			if (!angles)
			{
				// If the direction vector is not a unit vector, then normalize it.	
				if (!Vector3.Equals(direction, direction.normalized))	
				{
					direction = direction.normalized;
				}
			    // Scale the direction of the thrust acceleration by its magnitude, and sum it up with the gravitational acceleration.
				acceleration = gravitationalAcceleration + thrustAccelerationMagnitude * direction;
			}
			// Otherwise, the direction vector represents angles along the x, y, and z axes.
			else
			{
				// The trigonometric functions of the Mathf library only accept radians.
				// Therefore, check if the given angles are in radians, and if not, convert them.
				if (degrees)
				{
					direction *= Mathf.PI / 180.0f;
				}

				float sin_ux = Mathf.Sin(direction.x);
				float cos_ux = Mathf.Cos(direction.x);
				float sin_uy = Mathf.Sin(direction.y);
				float cos_uy = Mathf.Cos(direction.y);
				float sin_uz = Mathf.Sin(direction.z);
				float cos_uz = Mathf.Cos(direction.z);

				// Compute the unit vector from the given angles, and scale it by the thrust acceleration.
				acceleration.x = thrustAccelerationMagnitude * (cos_uz * sin_uy * sin_ux - cos_uy * sin_uz);
				acceleration.y = thrustAccelerationMagnitude * (cos_uz * cos_ux);
				acceleration.z = thrustAccelerationMagnitude * (sin_uz * sin_uy + cos_uz * cos_uy * sin_ux);
				// Finally, add the gravitational acceleration.
				acceleration += gravitationalAcceleration;
			}	

			return acceleration;
		}    




		/// <summary>
        /// Given the quadrotor's torques and moments of inertia, this method computes its angular acceleration in the inertial frame.
		/// For simplicity, it is assumed that the coriolis effect is negligible; hence, the result is the element-wise division of the torques by the inertia.
        /// </summary>
        /// <param name="torques">The torques acting on the quadrotor, around its x, y, and z axes.</param>
        /// <param name="inertia">The non-zero moments of inertia of the quadrotor, commonly referred to as Ixx, Iyy, and Izz.</param>
        /// <returns>The angular acceleration of the quadrotor in the inertial frame.</returns>
		public static Vector3 AngularAcceleration(Vector3 torques, Vector3 inertia)
		{
			return Vector3.Scale(torques, 1.0f / inertia);
		}    




		public static Vector4 TargetRotorSpeeds(float target_speed, Vector3 destination, Vector3 position,
										    	Vector3 velocity, Vector3 acceleration, float mass, float g,
												Vector3 inertia, Vector3 angular_position, Vector3 angular_velocity,
												float drag_factor, float thrust_factor, float rod_length, float yaw_factor,
												bool degrees=true)
		{
				// if (!degrees)
				// {
				// 	angular_position.x *= 180.0f / (float)Math.PI;
				// 	angular_position.y *= 180.0f / (float)Math.PI;
				// 	angular_position.z *= 180.0f / (float)Math.PI;
					
				// 	angular_velocity.x *= 180.0f / (float)Math.PI;
				// 	angular_velocity.y *= 180.0f / (float)Math.PI;
				// 	angular_velocity.z *= 180.0f / (float)Math.PI;
				// }	


                Vector3 targetVelocity = (destination - position).normalized;
                Debug.Log("targetVelocity BEFORE SPEED x: " + targetVelocity.x + " y: " + targetVelocity.y + " z: " + targetVelocity.z);
				Debug.Log("targetSpeed: " + target_speed);
				targetVelocity *= target_speed;
                Debug.Log("targetVelocity x: " + targetVelocity.x + " y: " + targetVelocity.y + " z: " + targetVelocity.z);
                Vector3 targetAcceleration = targetVelocity - velocity - g;
                Debug.Log("targetAcceleration x: " + targetAcceleration.x + " y: " + targetAcceleration.y + " z: " + targetAcceleration.z);


				// Cosines and sines cancel out, to give thrust_acceleration
				// float thrust_acceleration = Mathf.Sqrt(targetAcceleration.x * targetAcceleration.x
													//   + targetAcceleration.y * targetAcceleration.y
													//   + targetAcceleration.z * targetAcceleration.z);
				float thrust_acceleration = targetAcceleration.magnitude;

				Debug.Log("thrust_acceleration: " + thrust_acceleration);
				float thrust = mass * thrust_acceleration;
				Debug.Log("mass: " + mass);	
				Debug.Log("thrust: " + thrust);	
				Vector4 targetTorques;
                targetTorques.w = thrust;

				// targetAngularPosition.x /= thrust_acceleration;
				// targetAngularPosition.y -= g;
				// targetAngularPosition.y /= thrust_acceleration;
				// targetAngularPosition.z /= thrust_acceleration;
				// targetAngularPosition = targetAngularPosition.normalized;

				// Vector3 targetAngularPosition = targetAcceleration.normalized;

				Vector3 targetDirection = targetAcceleration.normalized;

				Vector3 targetAngularAcceleration = targetDirection;

				float Ixx = inertia.x;
				float Iyy = inertia.y;
				float Izz = inertia.z;
                Debug.Log("++++++++INERTIA++++++++++++ Ixx: " + inertia.x + " Iyy: " + inertia.y + " Izz: " + inertia.z);
				// float dux = angular_velocity.x;
				// float duy = angular_velocity.y;
				// float duz = angular_velocity.z;
				targetTorques.x = targetAngularAcceleration.x * Ixx;
				targetTorques.y = targetAngularAcceleration.y * Iyy;
				targetTorques.z = targetAngularAcceleration.z * Izz;
				// targetTorques.x = targetAngularAcceleration.x * Ixx	- (Iyy - Izz) * dux;
				// targetTorques.y = targetAngularAcceleration.y * Iyy	- (Izz - Ixx) * duy;
				// targetTorques.z = targetAngularAcceleration.z * Izz	- (Ixx - Iyy) * duz;
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
				float f1 = (W - Y - 2.0f * X) / 4.0f;
				float f3 = X + f1;
				float f2 = (Y - Z + f1 + f3) / 2.0f;
				float f4 = Z + f2;
			
				targetRotorSpeeds.w = f1;
				targetRotorSpeeds.x = f2;
				targetRotorSpeeds.y = f3;
				targetRotorSpeeds.z = f4;

				return targetRotorSpeeds;
		}	


	}
}