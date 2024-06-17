# Network Analyzer

The Network Analyzer is a network monitoring and troubleshooting tool built in C# for use with the Windows Operating System. It downloads as a single EXE for quick launch on any computer and while maintaining portability.

## Current Features
Note that for major features, a small **i** is displayed next to the feature which offers a brief explanation of the feature and any user input limitations if any exist.

### Latency Monitor
- Takes in up to five targets (IPv4 Address or DNS Name) and monitors their latency
- Keeps track of the lowest, highest and average latencies returned during the session
- Tracks how many packets were lost during the session for each target
- Reports on whether the connection was **UP**, **UNSTABLE** or **DOWN** during the session
- Ability to hover over a DNS name in the target list and view the IP Address the DNS name resolves to

**Status Breakdown**

The status indicates the condition of the ICMP packets returned from the user-defined targets and is calculated from a pool of up to 60 packets at any given time
- The status of **UP** is used when a target has had fewer than 12% of the packets returned as anything other than Success from the Ping Response
- The status of **UNSTABLE** is used when the target has had between 12% and 50% of its packets returned as anything other than Success from the Ping Response
- The status of **DOWN** is used when the target has had 50% or more of its packets returned as anything other than Success from the Ping Response

![Screenshot of the Latency Monitor feature with the Dark Mode theme.](/Images/latency_monitor.png)

### IP Scanner
- Scans the network for devices and returns information
- Supports both auto and manual scanning of networks
- Information returned from the scan: DNS Name, IP Address, MAC Address and NIC Manufacturer
- RDP, SSH and SMB functionality available for devices located in the scan

**Scanning Breakdown**
- Auto Mode
  - Determine the subnets to scan for devices based upon the active IP Addresses tied to the devices Network Interface Cards
  - Filters out APIPA, Link-Local and IPv6 addresses from the pool so as to provide the most accurate info with the least clutter

- Manual Mode
  - The subnets to scan are determined by the user input
  - Supported input includes the following:
	- IP Address followed by CIDR notation (e.g. 172.30.1.1/24)
	- IP Address followed by a Subnet Mask (e.g. 172.30.1.1 255.255.255.0)
	- IP Range separated by a hyphen (e.g. 172.30.1.1 - 172.30.1.57)

**Active Device Features**

These features will be displayed for a device if the port is available on the device.

Note that the port being available does not guarantee the connection can be established.
- Supports the following functionality for active devices:
  - RDP [attempts a connection using the Windows Rremote Desktop client]
  - SSH [attempts a connection via a Windows PowerShell console]
  - SMB [opens Windows File Explorer to the target destination and displays any available file shares]

![Screenshot of the IP Scanner feature with the Dark Mode theme.](/Images/ip_scanner.png)