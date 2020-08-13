namespace ISAACS
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public static class QuadrotorDynamics
	{

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
        /// <param name="dragFactor">(Optional) A damping factor for the thrust, representing resistance by the drag.</param>
        /// <param name="thrustFactor">(Optional) The contribution of each rotor force to the total thrust, as well as its x-torque ("roll force") and z-torque ("pitch force").</param>
        /// <param name="rodLength">(Optional) The distance between two opposing rotors, such as O(f1) and O(f3). It is assumed that O(f1)->O(f3) and O(f2)->O(f4) are equal.</param>
        /// <param name="yawFactor">(Optional) The contribution of each rotor force to the y-torque ("yaw force").</param>
        /// <returns>The signed mangitudes of the quadrotor's total thrust and torques, for the given rotor forces.</returns>
		public static Vector4 SpinRotors(Vector4 rotorForces, float rodLength=1.0f, float dragFactor=1.0f, float thrustFactor=1.0f, float yawFactor=1.0f)
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
        /// <param name="direction">The direction of acceleration (for example, the desired velocity subtracted by the current velocity).</param>
        /// <param name="gravitationalAcceleration">The acceleration due to gravity. Approximately equal to (0, -9.81, 0) on the Earth's surface.</param>
        /// <returns>The acceleration of the quadrotor in the inertial frame.</returns>
		public static Vector3 Acceleration(float thrust, float mass, Vector3 direction, Vector3 gravitationalAcceleration)
		{
			// If the direction vector is not a unit vector, normalize it.	
			if (!Vector3.Equals(direction, direction.normalized))
			{
				direction = direction.normalized;
			}

			Vector3 acceleration;

			// From Newton's law (F = ma), the acceleration of an object from an external force is equal to the force divided by the object's mass (a = F/m).
			float thrustAccelerationMagnitude = thrust / mass;
			// Scale the direction of the thrust acceleration by its magnitude, and sum it up with the gravitational acceleration.
			acceleration = gravitationalAcceleration + thrustAccelerationMagnitude * direction;

			return acceleration;
		}




		/// <summary>
        /// Given the quadrotor's thrust and mass, this method computes its acceleration in the inertial frame.
		/// Since Unity transforms refer to the inertial frame, the result can be directly integrated to solve for velocity and position.
        /// </summary>
        /// <param name="thrust">The signed magnitude of the total thrust exerted by the quadrotor.</param>
        /// <param name="mass">The total mass of the quadrotor.</param>
        /// <param name="normal">The normal vector coming out of the quadrotor (if the quadrotor is rotated correctly, then `transform.up` returns this vector).</param>
        /// <param name="velocity">The current velocity of the quadrotor.</param>
        /// <param name="gravitationalAcceleration">The acceleration due to gravity. Approximately equal to (0, -9.81, 0) on the Earth's surface.</param>
        /// <returns>The acceleration of the quadrotor in the inertial frame.</returns>
		public static Vector3 Acceleration(float thrust, float mass, Vector3 normal, Vector3 velocity, Vector3 gravitationalAcceleration)
		{
			Vector3 acceleration;

			// The direction of acceleration is from the direction of the (previous) velocity towards the direction of the quadrotor's normal (which is the direction of the new velocity).
			Vector3 direction = (velocity.magnitude * normal - velocity - gravitationalAcceleration).normalized;
			// From Newton's law (F = ma), the acceleration of an object from an external force is equal to the force divided by the object's mass (a = F/m).
			float thrustAccelerationMagnitude = thrust / mass;
			// Scale the direction of the thrust acceleration by its magnitude, and sum it up with the gravitational acceleration.
			acceleration = gravitationalAcceleration + thrustAccelerationMagnitude * direction;

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
			Vector3 inverseInertia;
			inverseInertia.x = 1.0f / inertia.x;
			inverseInertia.y = 1.0f / inertia.y;
			inverseInertia.z = 1.0f / inertia.z;
			return Vector3.Scale(torques, inverseInertia);
		}




		/// <summary>
        /// Given the quadrotor's torques and moments of inertia, this method computes its angular acceleration in the inertial frame.
		/// For simplicity, it is assumed that the coriolis effect is negligible; hence, the result is the element-wise division of the torques by the inertia.
        /// </summary>
        /// <param name="thrustForces">A vector holding the thrust (.w) and torques (.x, .y, .z) acting on the quadrotor.</param>
        /// <param name="inertia">The non-zero moments of inertia of the quadrotor, commonly referred to as Ixx, Iyy, and Izz.</param>
        /// <returns>The angular acceleration of the quadrotor in the inertial frame.</returns>
		public static Vector3 AngularAcceleration(Vector4 thrustForces, Vector3 inertia)
		{
			Vector3 torques;
			torques.x = thrustForces.x;
			torques.y = thrustForces.y;
			torques.z = thrustForces.z;

			Vector3 inverseInertia;
			inverseInertia.x = 1.0f / inertia.x;
			inverseInertia.y = 1.0f / inertia.y;
			inverseInertia.z = 1.0f / inertia.z;

			return Vector3.Scale(torques, inverseInertia);
		}




        /// <returns>An estimate for the rotor forces that will move the quadrotor closer to its destination.</returns>
		public static Vector4 TargetRotorForces(
												Vector3 destination,
												Vector3 position,
												float targetSpeed,
												Vector3 velocity,
												Vector3 angularVelocity,
												Vector3 inertia,
												float mass,
												Vector3 gravitationalAcceleration,
												float rodLength=1.0f,
												float dragFactor=1.0f,
												float thrustFactor=1.0f,
												float yawFactor=1.0f,
												bool degrees=true,
												bool manual=false
												)
		{
			// Compute the desired linear velocity.
			Vector3 targetVelocity = targetSpeed * (destination - position).normalized;

			// Compute the desired angular velocity, using the specified method (manual, or Unity built-in).
			Vector3 targetAngularVelocity;
			if (manual)
			{
				// Compute the orthogonal axis to the velocity and the desired velocity.
				Vector3 axis = Vector3.Cross(velocity, targetVelocity).normalized;
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;
				// Compute the angle between the velocity and the desired velocity, along the orthogonal axis.
				float angle = Vector3.SignedAngle(velocity, targetVelocity, axis) * Mathf.PI / 180.0f;

				// Compute where the 1D axis-angle function maps to the 2D "Euler plane".
				float spot = x * y * (1.0f - Mathf.Cos(angle)) + z * Mathf.Sin(angle);

				// Check if the mapped spot corresponds to a North pole singularity, and calculate the Euler angles between the current and desired velocities accordingly.
				if (spot > 0.999f)
				{
					targetAngularVelocity.x = 180.0f / Mathf.PI * 2.0f * Mathf.Atan2(x * Mathf.Sin(angle / 2.0f), Mathf.Cos(angle / 2.0f));
					targetAngularVelocity.y = 180.0f / Mathf.PI * Mathf.Asin(x * y * (1.0f - Mathf.Cos(angle)) + z * Mathf.Sin(angle));
					targetAngularVelocity.z = 0.0f;
				}
				// Check if the mapped spot corresponds to a South pole singularity, and calculate the Euler angles between the current and desired velocities accordingly.
				else if (spot < -0.999f)
				{
					targetAngularVelocity.x = 180.0f / Mathf.PI * -2.0f * Mathf.Atan2(x * Mathf.Sin(angle / 2.0f), Mathf.Cos(angle / 2.0f));
					targetAngularVelocity.y = 180.0f / Mathf.PI * Mathf.Asin(x * y * (1.0f - Mathf.Cos(angle)) + z * Mathf.Sin(angle));
					targetAngularVelocity.z = 0.0f;
				}
				// Otherwise, this is a regular spot, meaning that the Euler angles can be calculated using the standard formula.
				else
				{
					targetAngularVelocity.x = 180.0f / Mathf.PI * Mathf.Atan2(y * Mathf.Sin(angle) - x * z * (1.0f - Mathf.Cos(angle)), 1.0f - (y * y + z * z) * (1.0f - Mathf.Cos(angle)));
					targetAngularVelocity.y = 180.0f / Mathf.PI * Mathf.Asin(x * y * (1.0f - Mathf.Cos(angle)) + z * Mathf.Sin(angle));
					targetAngularVelocity.z = 180.0f / Mathf.PI * Mathf.Atan2(x * Mathf.Sin(angle) - y * z * (1.0f - Mathf.Cos(angle)), 1.0f - (x * x + z * z) * (1.0f - Mathf.Cos(angle)));
				}
			}
			else
			{
				// Use the Unity built-in.	
				targetAngularVelocity = Quaternion.FromToRotation(velocity, targetVelocity).eulerAngles;
			}


			// Compute the desired linear acceleration.
			Vector3 targetAcceleration = targetVelocity - velocity - gravitationalAcceleration;

			// Compute the desired angular acceleration.
			Vector3 targetAngularAcceleration = targetAngularVelocity - angularVelocity;


			// The desired acceleration from thrust is the magnitude of the desired acceleration vector.
			float thrustAcceleration = targetAcceleration.magnitude;
			// Compute the desired total thrust to be generated by the rotors, using Newton's Law (F = ma).
			float thrust = mass * thrustAcceleration;

			// Compute the target torques to be generated by the torques, by inverting the equation of angular acceleration.
			Vector3 torques = Vector3.Scale(targetAngularAcceleration, inertia);


			// Compute the constants needed to solve for the rotor forces.
			float W = thrust / (dragFactor * thrustFactor);
			float X = torques.x / (rodLength * thrustFactor);
			float Y = torques.y / yawFactor;
			float Z = torques.z / (rodLength * thrustFactor);

			// Solve for the rotor forces.
			float f1 = (W - Y - 2.0f * X) / 4.0f;
			float f3 = X + f1;
			float f2 = (Y - Z + f1 + f3) / 2.0f;
			float f4 = Z + f2;

			Vector4 forces;
			forces.w = f1;
			forces.x = f2;
			forces.y = f3;
			forces.z = f4;

			return forces;
		}

	}
}