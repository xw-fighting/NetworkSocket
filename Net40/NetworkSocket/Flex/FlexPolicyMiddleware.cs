﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkSocket.Flex
{
    /// <summary>
    /// 表示Flex通讯策略XML文件中间件
    /// </summary>
    public class FlexPolicyMiddleware : IMiddleware
    {
        /// <summary>
        /// 下一个中间件
        /// </summary>
        public IMiddleware Next { get; set; }

        /// <summary>
        /// 执行中间件          
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        Task IMiddleware.Invoke(IContenxt context)
        {
            if (context.Session.Protocol != null || context.Buffer.Length != 23)
            {
                return this.Next.Invoke(context);
            }

            context.Buffer.Position = 0;
            var request = context.Buffer.ReadString(Encoding.ASCII);
            if (string.Equals(request, "<policy-file-request/>\0", StringComparison.OrdinalIgnoreCase))
            {
                return new Task(() => this.SendPolicyXML(context));
            }
            return this.Next.Invoke(context);
        }

        /// <summary>
        /// 发送策略文件
        /// </summary>
        /// <param name="context">上下文</param>
        private void SendPolicyXML(IContenxt context)
        {
            try
            {
                var policyXml = this.GeneratePolicyXml();
                var bytes = Encoding.UTF8.GetBytes(policyXml.ToCharArray());
                context.Session.SendAsync(new ByteRange(bytes));
            }
            catch (Exception)
            {
            }
            finally
            {
                context.Session.Close();
            }
        }

        /// <summary>
        /// 生成策略xml
        /// </summary>
        /// <returns></returns>
        protected virtual string GeneratePolicyXml()
        {
            return new StringBuilder()
                .AppendLine("<cross-domain-policy>")
                .AppendLine("<allow-access-from domain=\"*\" to-ports=\"*\"/>")
                .AppendLine("</cross-domain-policy>\0")
                .ToString();
        }
    }
}
