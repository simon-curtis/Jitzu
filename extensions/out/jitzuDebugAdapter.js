"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
const debugadapter_1 = require("@vscode/debugadapter");
const fs = __importStar(require("node:fs"));
const path = __importStar(require("node:path"));
const node_child_process_1 = require("node:child_process");
class JitzuDebugSession extends debugadapter_1.LoggingDebugSession {
    constructor() {
        super();
        this.setDebuggerLinesStartAt1(true);
        this.setDebuggerColumnsStartAt1(true);
    }
    initializeRequest(response, _args) {
        // declare minimal capabilities
        response.body = response.body || {};
        response.body.supportsTerminateRequest = true;
        // tell VS Code we're ready
        this.sendResponse(response);
        this.sendEvent(new debugadapter_1.InitializedEvent());
    }
    async launchRequest(response, args) {
        try {
            const cwd = args.cwd || process.cwd();
            const prog = path.isAbsolute(args.program)
                ? args.program
                : path.join(cwd, args.program);
            // trivial validation
            if (!fs.existsSync(prog)) {
                this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] File not found: ${prog}\n`, "stderr"));
                this.sendResponse(response);
                // end session
                this.sendEvent(new debugadapter_1.TerminatedEvent());
                return;
            }
            // Log what we’re about to do
            this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] Launching: ${prog}\n`, "console"));
            this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] args: ${(args.args || []).join(" ")}\n`, "console"));
            this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] cwd: ${cwd}\n`, "console"));
            // Option B: spawn your real runtime (replace "jitzu" with your runner)
            // Example assumes a 'jitzu' CLI on PATH: jitzu <program> <args...>
            const runner = "D:\\git\\jitzu\\Jitzu.Interpreter\\bin\\Publish\\jz.exe";
            const childArgs = [prog, ...(args.args || [])];
            this.process = (0, node_child_process_1.spawn)(runner, childArgs, {
                cwd,
                shell: process.platform === "win32"
            });
            this.process.stdout?.on("data", (d) => {
                this.sendEvent(new debugadapter_1.OutputEvent(d.toString(), "stdout"));
            });
            this.process.stderr?.on("data", (d) => {
                this.sendEvent(new debugadapter_1.OutputEvent(d.toString(), "stderr"));
            });
            this.process.on("exit", (code) => {
                this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] Exit code: ${code}\n`, "console"));
                this.sendEvent(new debugadapter_1.TerminatedEvent());
            });
            this.process.on("error", (err) => {
                this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] Spawn error: ${err.message}\n`, "stderr"));
                this.sendEvent(new debugadapter_1.TerminatedEvent());
            });
            this.sendResponse(response);
        }
        catch (e) {
            this.sendEvent(new debugadapter_1.OutputEvent(`[jitzu] Launch error: ${e?.message}\n`, "stderr"));
            this.sendResponse(response);
            this.sendEvent(new debugadapter_1.TerminatedEvent());
        }
    }
    disconnectRequest(response, _args) {
        if (this.process && !this.process.killed) {
            try {
                this.process.kill();
            }
            catch { }
        }
        this.sendResponse(response);
    }
}
// Entry point: run as a debug adapter over stdio
debugadapter_1.LoggingDebugSession.run(JitzuDebugSession);
