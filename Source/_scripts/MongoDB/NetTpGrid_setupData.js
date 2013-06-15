use p3Grid;

var doc = {
  "_id" : "TestApp1",
  "IsProcess" : true,
  "ConfigId" : "f35151b0-9f66-4bf4-83ea-5cfb0dc44db6",
  "RunOn" : ".*",
  "Worker" : null,
  "WorkerStrategy" : "DontSupervise",
  "SupervisionStrategy" : {
    "_t" : "PermanentNodeSupervisionStrategy",
    "MaxRestarts" : 12,
    "MaxTime" : "00:01:01",
    "DelayMsTimes" : null
  },
  "RestartStrategy" : {
    "_t" : "OneForOneNodeRestartStrategy"
  },
  "Nodes" : [{
      "IsProcess" : true,
      "ConfigId" : "21220ce6-3ef3-4b6f-a07d-d687f1231784",
      "_id" : "ChildWithWorker",
      "RunOn" : null,
      "Worker" : "Pcubed.Framework.Tester.TickNodeWorker, Pcubed.Framework.Tester",
      "WorkerStrategy" : "DontSupervise",
      "SupervisionStrategy" : {
        "_t" : "TemporaryNodeSupervisionStrategy"
      },
      "RestartStrategy" : {
        "_t" : "OneForOneNodeRestartStrategy"
      },
      "Nodes" : null,
      "Processes" : []
    },{
      "IsProcess" : true,
      "ConfigId" : "b9b7c9cf-4fac-49c2-bae1-b4c79538a87f",
      "_id" : "ChildWithHotSwapHanlder",
      "RunOn" : null,
      "Worker" : "Pcubed.Framework.Grid.Config.ConfigHotSwapNodeWorker, Pcubed.Framework",
	  "NodeSettings": {
	  	"HotSwapLoaderType": null,
		"HotSwapLoaderTypeAsmToScan": "Pcubed.Framework.Tester"
	  }
      "WorkerStrategy" : "Supervise",
      "SupervisionStrategy" : {
        "_t" : "PermanentNodeSupervisionStrategy"
      },
      "RestartStrategy" : {
        "_t" : "OneForOneNodeRestartStrategy"
      },
      "Nodes" : null,
      "Processes" : []
    },{
      "IsProcess" : true,
      "ConfigId" : "a66a9578-6a2d-4fb0-bcdd-35f825c8d1f1",
      "_id" : "ChildWithProcesses",
      "RunOn" : null,
      "Worker" : "",
      "WorkerStrategy" : "DontSupervise",
      "SupervisionStrategy" : {
        "_t" : "PermanentNodeSupervisionStrategy"
      },
      "RestartStrategy" : {
        "_t" : "OneForOneNodeRestartStrategy"
      },
      "Nodes" : null,
      "Processes" : [{
	      "Name" : "calc",
	      "FileName" : "C:\\Windows\\System32\\calc.exe",
	      "WorkingDirectory" : "C:\\Windows\\System32"
	    }, {
	      "Name" : "ver",
	      "FileName" : "C:\\Windows\\System32\\winver.exe",
	      "WorkingDirectory" : "C:\\Windows\\System32"
	    }]
    }],
  "Processes" : []
};

db.config.update( {_id: "TestApp1"}, doc, true );
//------------------------------------------------------------

