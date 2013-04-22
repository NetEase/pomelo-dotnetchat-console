using System; 
using SimpleJson;
using System.Timers;
using System.IO;
using Pomelo.DotNetClient;

namespace Pomelo.DotNetClient.Chat
{
	class Test
	{
		static PomeloClient pc;
		static string user;

		public static JsonObject read(string name){
			StreamReader file = new StreamReader(name);
			
			String str = file.ReadToEnd();
			
			return (JsonObject)SimpleJson.SimpleJson.DeserializeObject(str);
		}

		public static void Main(){
			JsonObject server = read ("./server.json");
			string host = (string)server["host"];
			int port = Convert.ToInt32(server["port"]);

			Console.WriteLine ("before init");
			
			pc = new PomeloClient(host, port);
			
			Console.WriteLine ("before connect");
			pc.connect(null, delegate(JsonObject data){
				Console.WriteLine ("after connect " + data.ToString());
				JsonObject msg = new JsonObject();

				msg["uid"] = 1;
				pc.request("gate.gateHandler.queryEntry", msg, onQuery);
			});
			 
			while(true) {	
				string str = Console.ReadLine();
				if(str != null){
					send (str);
				}
				System.Threading.Thread.Sleep(100);
			}
		}

		public static void onQuery(JsonObject result){
			if(Convert.ToInt32(result["code"]) == 200){
				string host = (string)result["host"];
				int port = Convert.ToInt32(result["port"]);
				pc.disconnect();
				startConnect(host, port);
			}
		}

		private static void startConnect(string host, int port){
			pc = new PomeloClient(host, port);
			pc.connect(null, delegate(JsonObject data){
				pc.on("onChat", delegate(JsonObject msg) {
					Console.WriteLine("{0} : {1}.", msg["from"], msg["msg"]);
				});
				pc.on("onAdd", delegate(JsonObject msg) {
					Console.WriteLine("user {0} come in to the room.", msg["user"]);
				});
				pc.on("onLeave", delegate(JsonObject msg) {
					Console.WriteLine("user {0} leave the room", msg["user"]);
				});
				pc.on("disconnect", delegate(JsonObject msg) {
					Console.WriteLine("disconnect : " + msg);
				});

				//kick ();
				login ();
			});	
		}

		public static void kick(){
			pc.request("connector.entryHandler.onUserLeave", delegate (JsonObject data) {
				Console.WriteLine("userLeave " + data);
			});
		}

		public static void login(){
			user = "pomelo" + DateTime.Now.Millisecond;

			JsonObject args = new JsonObject();
			args["username"] = user;
			args["rid"] = "pomelo";

			pc.request("connector.entryHandler.enter", args, delegate (JsonObject data) {
				Console.WriteLine("user : {0} Log in.", user);
			});
		}

		public static void send(string message){
			JsonObject args = new JsonObject();
			args["content"] = message;
			args["target"] = "*";

			pc.notify("chat.chatHandler.send", args);
		}
	}
}
