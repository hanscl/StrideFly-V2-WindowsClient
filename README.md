# StrideFly-V2-WindowsClient
Code for StrideFly Tracking Client (requires XBee radio on COM Port to receive GPS positions from tracker)

StrideFly is a live tracking system for outdoor running events. Prototype V2 implemented a closed system (i.e. no web or other connectivity required) that used 900Mhz XBee radios to transmit GPS positions of runners to the tracking client (a windows tablet) at the base camp.

The code in this repository is for the Windows-based client that receives the tracking positions and shows the runner's position on a pre-configured map with corresponding satellite image. The client software utilizes the high-level WPF (Windows Presentation Framework) and also makes use of a positioning/mapping component provided by Telerik.

Positions are received from an XBee radio that needs to be connected to the COM port (or virtual com port over USB) on the client machine.
