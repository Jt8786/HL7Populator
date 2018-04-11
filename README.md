# HL7Populator
This project is meant to automate the sending/receiving of generic HL7 messages for populating test data/testing HL7 flow. This was built with a radiology focus, but it can be altered to use different procedure codes

## Implementation
To implement, you can reference the server object in the main project. If you'd like to be able to manipulate messages before being sent and/or process the received messages, you can implement the abstract MessageProcessor class.
