# HL7Populator
This project is meant to automate the sending/receiving of generic HL7 messages for populating test data/testing HL7 flow. This was built with a radiology focus, but it can be altered to use different procedure codes

## Implementation
To implement, you can create a Server object from the main project. If you'd like to be able to manipulate messages before being sent and/or process the received messages, you can implement the abstract MessageProcessor class, passing it a server object

## Logging
HL7 logging can be enabled, outputting the data to C:\ProgramData\HL7Populator\HL7Populator.log by passing a logging level of INFO when creating the Server object.
