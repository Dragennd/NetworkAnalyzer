# NetworkAnalyzer

The Network Analyzer is a network monitoring and troubleshooting tool built in C# and intended to be downloaded and launched for quick and easy use.
No installation is required with this application, it downloads as a single EXE.

## Current Features
- Latency Monitor
  - Takes in up to five targets (IPv4 Address or DNS Name) and monitors their latency
  - Keeps track of the lowest, highest and average latencies returned during the session
  - Tracks how many packets were lost during the session
  - Reports on whether the connection was up, unstable or down during the session

- IP Scanner
  - Scans the network for devices and returns information
  - Supports both auto and manual scanning of networks
    - Auto Scanning: Scans the network tied to your device's active NICs (not link-local, IPv6 or APIPA)
    - Manual Scanning: Scans whatever networks are entered into the provided field (takes in IPv4 Addressses with CIDR notation, IPv4 Addresses with a Subnet Mask or specified IPv4 Address ranges)
  - Information returned from the scan: DNS Name, IP Address, MAC Address and NIC Manufacturer
  - Functionality available for devices located in the scan
    - Able to establish a RDP or SSH session with devices that have those features enabled
    - Able to launch File Explorer directly to the location of any SMB shares on the devices which have them
