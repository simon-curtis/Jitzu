import {
    LoggingDebugSession,
    InitializedEvent,
    TerminatedEvent,
    OutputEvent
} from "@vscode/debugadapter";
import { DebugProtocol } from "@vscode/debugprotocol";
import * as fs from "node:fs";
import * as path from "node:path";
import { spawn } from "node:child_process";

interface LaunchRequestArguments extends DebugProtocol.LaunchRequestArguments {
    program: string; // path to .jz
    args?: string[];
    cwd?: string;
}

class JitzuDebugSession extends LoggingDebugSession {
    private process?: ReturnType<typeof spawn>;

    public constructor() {
        super();
        this.setDebuggerLinesStartAt1(true);
        this.setDebuggerColumnsStartAt1(true);
    }

    protected initializeRequest(
        response: DebugProtocol.InitializeResponse,
        _args: DebugProtocol.InitializeRequestArguments
    ): void {
        // declare minimal capabilities
        response.body = response.body || {};
        response.body.supportsTerminateRequest = true;

        // tell VS Code we're ready
        this.sendResponse(response);
        this.sendEvent(new InitializedEvent());
    }

    protected async launchRequest(
        response: DebugProtocol.LaunchResponse,
        args: LaunchRequestArguments
    ): Promise<void> {
        try {
            const cwd = args.cwd || process.cwd();
            const prog = path.isAbsolute(args.program)
                ? args.program
                : path.join(cwd, args.program);

            // trivial validation
            if (!fs.existsSync(prog)) {
                this.sendEvent(
                    new OutputEvent(`[jitzu] File not found: ${prog}\n`, "stderr")
                );
                this.sendResponse(response);
                // end session
                this.sendEvent(new TerminatedEvent());
                return;
            }

            // Log what we’re about to do
            this.sendEvent(
                new OutputEvent(`[jitzu] Launching: ${prog}\n`, "console")
            );
            this.sendEvent(
                new OutputEvent(
                    `[jitzu] args: ${(args.args || []).join(" ")}\n`,
                    "console"
                )
            );
            this.sendEvent(new OutputEvent(`[jitzu] cwd: ${cwd}\n`, "console"));

            // Option B: spawn your real runtime (replace "jitzu" with your runner)
            // Example assumes a 'jitzu' CLI on PATH: jitzu <program> <args...>
            const runner = "D:\\git\\jitzu\\Jitzu.Interpreter\\bin\\Publish\\jz.exe";
            const childArgs = [prog, ...(args.args || [])];

            this.process = spawn(runner, childArgs, {
                cwd,
                shell: process.platform === "win32"
            });

            this.process.stdout?.on("data", (d) => {
                this.sendEvent(new OutputEvent(d.toString(), "stdout"));
            });
            this.process.stderr?.on("data", (d) => {
                this.sendEvent(new OutputEvent(d.toString(), "stderr"));
            });
            this.process.on("exit", (code) => {
                this.sendEvent(
                    new OutputEvent(`[jitzu] Exit code: ${code}\n`, "console")
                );
                this.sendEvent(new TerminatedEvent());
            });
            this.process.on("error", (err) => {
                this.sendEvent(
                    new OutputEvent(`[jitzu] Spawn error: ${err.message}\n`, "stderr")
                );
                this.sendEvent(new TerminatedEvent());
            });

            this.sendResponse(response);
        } catch (e: any) {
            this.sendEvent(
                new OutputEvent(`[jitzu] Launch error: ${e?.message}\n`, "stderr")
            );
            this.sendResponse(response);
            this.sendEvent(new TerminatedEvent());
        }
    }

    protected disconnectRequest(
        response: DebugProtocol.DisconnectResponse,
        _args: DebugProtocol.DisconnectArguments
    ): void {
        if (this.process && !this.process.killed) {
            try {
                this.process.kill();
            } catch { }
        }
        this.sendResponse(response);
    }
}

// Entry point: run as a debug adapter over stdio
LoggingDebugSession.run(JitzuDebugSession);