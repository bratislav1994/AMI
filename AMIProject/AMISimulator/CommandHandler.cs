using Automatak.DNP3.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMISimulator
{
    public class CommandHandler : ICommandHandler
    {
        private bool canExecute = false;
        private AMISimulator simulator;

        public CommandHandler(AMISimulator simulator)
        {
            this.simulator = simulator;
        }
        
        public CommandStatus Operate(AnalogOutputFloat32 command, ushort index, OperateType opType)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Operate(AnalogOutputDouble64 command, ushort index, OperateType opType)
        {
            return this.simulator.SetNewSetPointForPowerTransformer(command.value, index) ? CommandStatus.SUCCESS : CommandStatus.CANCELLED;
        }

        public CommandStatus Operate(AnalogOutputInt16 command, ushort index, OperateType opType)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Operate(AnalogOutputInt32 command, ushort index, OperateType opType)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Operate(ControlRelayOutputBlock command, ushort index, OperateType opType)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Select(AnalogOutputInt16 command, ushort index)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Select(AnalogOutputDouble64 command, ushort index)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Select(AnalogOutputFloat32 command, ushort index)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Select(AnalogOutputInt32 command, ushort index)
        {
            throw new NotImplementedException();
        }

        public CommandStatus Select(ControlRelayOutputBlock command, ushort index)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            this.canExecute = true;
        }

        public void End()
        {
            this.canExecute = false;
        }
    }
}
