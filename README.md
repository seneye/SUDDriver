# seneye USB Device (SUD) Driver Developer Information

## Background

Welcome to the [seneye USB Device (SUD)](http://answers.seneye.com/en/Seneye_Products/seneye_USB_device) Driver Developer Information.

_If you are looking for the the instructions for normal use of the SUD then please click [here](http://answers.seneye.com/en) as you are in the wrong place._

The information in this repository provides all the details required to produce a stand-alone program to communicate with the SUD over USB. Source code examples are provided in ".NET" and "c" languages.


## Outcome

When the code is implemented sucessfully you will be able to:

- Find the SUD on the host computer over USB
- Obtain the serial number from the SUD
- Receive a lightmeter reading from the SUD*
- Call a reading of pH, NH3, in/out of water, & temperature from the SUD 
- Activate a seneye slide to the SUD

Full details can be found [here](https://github.com/seneye/SUDDriver/wiki)

## Example code

The example code provided functions in a command-line enviroment. No direct support is offered from seneye for development beyond the information in this repository. 

-----

## Notes

_The sample code is only compatible with a version 2 firmware SUD. If you are unsure which version of the fimware you have, you can [E-mail](mailto:support@seneye.com) the serial number of your device checked. The serial number is located on the label that is on the cable next to the USB connector._

_* only avalible with the seneye Reef model_