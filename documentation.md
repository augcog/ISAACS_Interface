# Documentation
## Manifold Setup

### [Hardware](https://dl.djicdn.com/downloads/manifold-2/20190528/Manifold_2_User_Guide_v1.0_EN.pdf)

### [Software](https://github.com/dji-sdk/Onboard-SDK-ROS)
- On boot, a new Manifold requires a lot of updates (this happens automatically and will block apt-get along with get really hot for 15 or so minutes).
- ROS Kinetic should come pre-installed but feel free to follow the instructions on ROS's wiki.
- sudo apt install ros-kinetic-rosbridge-server
- Install Onboard-SDK-ROS
    - We are using Onboard-SDK-ROS commit f424a07, the commit to the master branch right before version 4.0.0 removed support for the M210 RTK v1.4
    - Create a Catkin workspace
        - mkdir catkin_ws
        - cd catkin_ws
        - catkin_init
    - clone the Onboard-SDK-ROS repo into catkin_ws/src
    - cd Onboard-SDK-ROS
    - git checkout f424a07
    - mkdir build
    - cd build
    - cmake ../cmake-modules
    - make
    - cd ~/catkin_ws
    - catkin_make

- Enabling Advanced Sensing:
    - On line 19 of Onboard-SDK-ROS/cmake-modules/External_djiosdkcore.cmake, either turn `-DADVANCED_SENSING=ON`(default) or `OFF`
