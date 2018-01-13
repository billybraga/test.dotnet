using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace TestPerf.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            
            Func<string, string>[] handlers = new Func<string, string>[]
            {
                table => "SELECT * from " + table,
                table => "SELECT count(*) from " + table,
                table => "SELECT * from " + table + " LIMIT 10, 1000"
            };

            app.Run(async (context) =>
            {
                try
                {
                    var sb = new StringBuilder();
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    context.Response.Headers.Add("X-Version", "1.1");
                    using (var conn = new MySqlConnection(
                        "server=127.0.0.1;uid=bikajewels;pwd=bikajewels;database=bikajewels;Convert Zero Datetime=True"))
                    {
                        await conn.OpenAsync();
                        
                        for (var i = 0; i < this.tables.Length; i++)
                        {
                            var table = this.tables[i];
                            var sql = handlers[i % handlers.Length](table);
                            sb.AppendLine();
                            sb.Append(sql);
                            sb.AppendLine();
                            using (var command = conn.CreateCommand())
                            {
                                command.CommandText = sql;
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        for (var j = 0; j < reader.FieldCount; j++)
                                        {
                                            sb.Append(GetValue(reader, j));
                                            sb.Append(" ");
                                        }

                                        sb.AppendLine();
                                    }
                                }
                            }
                        }
                    }

                    await context.Response.WriteAsync(sb.ToString());
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.ToString());
                }
            });
        }

        private string GetValue(DbDataReader reader, int i)
        {
            return reader.GetFieldValue<object>(i).ToString();
        }

        private string[] tables = new[]
        {
            "kfm_new_directories",
            "kfm_new_files",
            "kfm_new_files_images",
            "kfm_new_files_images_thumbs",
            "kfm_new_parameters",
            "kfm_new_plugin_extensions",
            "kfm_new_session",
            "kfm_new_session_vars",
            "kfm_new_settings",
            "kfm_new_tagged_files",
            "kfm_new_tags",
            "kfm_new_translations",
            "kfm_new_users",
            "wp_commentmeta",
            "wp_comments",
            "wp_icl_cms_nav_cache",
            "wp_icl_content_status",
            "wp_icl_core_status",
            "wp_icl_flags",
            "wp_icl_languages",
            "wp_icl_languages_translations",
            "wp_icl_locale_map",
            "wp_icl_message_status",
            "wp_icl_node",
            "wp_icl_reminders",
            "wp_icl_string_positions",
            "wp_icl_string_status",
            "wp_icl_string_translations",
            "wp_icl_strings",
            "wp_icl_translate",
            "wp_icl_translate_job",
            "wp_icl_translation_status",
            "wp_icl_translations",
            "wp_links",
            "wp_options",
            "wp_postmeta",
            "wp_posts",
            "wp_term_relationships",
            "wp_term_taxonomy",
            "wp_terms",
            "wp_translate_dict",
            "wp_translate_langs",
            "wp_usermeta",
            "wp_users"
        };
    }
}