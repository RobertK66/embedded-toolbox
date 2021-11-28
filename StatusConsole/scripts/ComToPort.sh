#!/bin/bash

# With this script you can redirect a local USB Com device (e.g. FTDI USB_RS232 Converter) 
# to be conectable via a remote host:port socket connection.

if [[ -z $1 ]]; then
  CTP_DEVICE='/dev/ttyUSB0'
else
  CTP_DEVICE=$1
fi

if [[ -z $2 ]]; then
  CTP_PORT='9801'
else
  CTP_PORT=$2
fi

#
# Lets make this standard tty (as it is initialized after USB plugin) 
# usable as simple COM port - writing and reading from UART rx/tx without any 'linediscipline'
# applied by the tty device (e.g. local echo on RX gives endless loop if RX/TX is hardware loopbacked.....)
#
stty -F $CTP_DEVICE -isig -icanon -echo -echoe -echok
stty -F $CTP_DEVICE

while :; do
echo redirecting $CTP_DEVICE to port: $CTP_PORT
nc -l $CTP_PORT > $CTP_DEVICE < $CTP_DEVICE ;
echo port closed ... reopening ...
done
