pragma solidity ^0.5.0;

contract ClinicalTrial {
    
    struct Patient{
        bool completed; 
        string key;
    }
    
    Patient[] patients;
    
    function joinTrial(string memory s) public {
        patients.push(Patient(false,s));
    }
    
    function compare(string memory _a, string memory _b) internal returns (bool) {
        bytes memory a = bytes(_a);
        bytes memory b = bytes(_b);
        return (keccak256(a)==keccak256(b));
    }
    
    function patientExist(string memory s) public returns (bool) {
        for (uint i=0; i<patients.length; ++i) {
            if (compare(s,patients[i].key)) {
                return true;
            }
        }
    
        return false;
    }
    
    function finishTrial(string memory k) public {
        for (uint i=0; i<patients.length; ++i) {
            if (compare(k,patients[i].key)) {
                patients[i].completed = true;
            }
        }
    }
    
    function isTrialFinished() public returns (bool) {
        for (uint i=0; i<patients.length; ++i) {
            if (!patients[i].completed) {
                return false;
            }
        }
        return true;
    }
    
}



