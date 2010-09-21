TelnetD
=======

A rudimentary telnet server daemon for Windows.

Usage: telnetd.exe <address> <port> [password]

For example, on your server, you might run telnetd.exe 127.0.0.1 8080

Then on your client, you can connect with telnet myserver 8080 to get
command line access and run the usual commands like dir, set and type
etc.

CTRL D to exit and disconnect.

Warning - the telnet protocol sends all data over the wire in clear
text meaning that this software is probably not secure enough for
production applications. It is a useful occasional debugging tool
though!

No it isn't RFC854 compliant.

Rob Blackwell
Todd MacDonald

September 2010
