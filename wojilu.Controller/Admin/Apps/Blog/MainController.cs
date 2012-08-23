/*
 * Copyright (c) 2010, www.wojilu.com. All rights reserved.
 */

using System;
using System.Collections;
using System.Collections.Generic;

using wojilu.Web.Mvc;
using wojilu.Web.Mvc.Attr;
using wojilu.Apps.Blog.Service;
using wojilu.Apps.Blog.Domain;
using wojilu.Apps.Blog.Interface;
using wojilu.Members.Sites.Interface;
using wojilu.Members.Sites.Service;
using wojilu.Members.Users.Domain;
using wojilu.Web.Controller.Security;
using wojilu.Members.Interface;
using wojilu.Members.Sites.Domain;

namespace wojilu.Web.Controller.Admin.Apps.Blog {

    [App( typeof( BlogApp ) )]
    public partial class MainController : ControllerBase {

        public IBlogPostService postService { get; set; }
        public IPickedService pickedService { get; set; }
        public ISysBlogService sysblogService { get; set; }
        public IAdminLogService<SiteLog> logService { get; set; }
        public IBlogSysCategoryService categoryService { get; set; }

        public MainController() {
            postService = new BlogPostService();
            pickedService = new PickedService();
            sysblogService = new SysBlogService();
            logService = new SiteLogService();
            categoryService = new BlogSysCategoryService();
        }

        // TODO 搜索功能：根据作者、根据时间(最近一个月)、根据阅读量、根据评论数、
        public void Index( int id ) {

            target( Admin );

            DataPage<BlogPost> list = sysblogService.GetSysPageByCategory( id, 36 );
            bindPosts( list );

            setCategoryDropList();
        }


        private void setCategoryDropList() {
            List<BlogSysCategory> categories = categoryService.GetAll();
            List<BlogSysCategory> list = addSelectInfo( categories );
            dropList( "adminDropCategoryList", list, "Name=Id", null );
        }

        private List<BlogSysCategory> addSelectInfo( List<BlogSysCategory> categories ) {
            BlogSysCategory category = new BlogSysCategory();
            category.Id = -1;
            category.Name = lang( "setCategory" );

            List<BlogSysCategory> list = new List<BlogSysCategory>();
            list.Add( category );
            foreach (BlogSysCategory cat in categories) {
                list.Add( cat );
            }
            return list;
        }


        public void Picked() {
            target( Admin );
            DataPage<BlogPost> list = pickedService.GetAll();
            bindPosts( list );
        }

        public void Trash() {
            target( Admin );

            DataPage<BlogPost> list = sysblogService.GetSysPageTrash();
            bindPosts( list );
        }

        [HttpPost, DbTransaction]
        public void Admin() {

            String ids = ctx.PostIdList( "choice" );
            String cmd = ctx.Post( "action" );
            int categoryId = ctx.PostInt( "categoryId" );

            String condition = string.Format( "Id in ({0}) ", ids );

            if (strUtil.IsNullOrEmpty( cmd ) ) {
                echoText( lang( "exCmd" ) );
                return;
            }

            if (strUtil.IsNullOrEmpty( ids )) {
                echoText( lang( "plsSelect" ) );
                return;
            }

            if ("pick".Equals( cmd )) {
                pickedService.PickPost( ids );
                log( SiteLogString.PickBlogPost(), ids );
                echoAjaxOk();
            }
            else if ("unpick".Equals( cmd )) {
                pickedService.UnPickPost( ids );
                log( SiteLogString.UnPickBlogPost(), ids );
                echoAjaxOk();
            }
            else if ("delete".Equals( cmd )) {
                sysblogService.Delete( ids );
                log( SiteLogString.DeleteBlogPost(), ids );
                echoAjaxOk();
            }
            else if ("undelete".Equals( cmd )) {
                sysblogService.UnDelete( ids );
                log( SiteLogString.UnDeleteBlogPost(), ids );
                echoAjaxOk();
            }
            else if ("deletetrue".Equals( cmd )) {
                sysblogService.DeleteTrue( ids );
                log( SiteLogString.DeleteBlogPostTrue(), ids );
                echoAjaxOk();
            }
            else if ("category".Equals( cmd )) {
                if (categoryId <= 0) {
                    actionContent( lang( "exCategoryNotFound" ) );
                    return;
                }
                BlogPost.updateBatch( "set SysCategoryId=" + categoryId, condition );
                log( SiteLogString.MoveBlogPost(), ids );

                echoAjaxOk();
            }
            else
                echoText( lang( "exUnknowCmd" ) );

        }


        [HttpDelete, DbTransaction]
        public void Delete( int id ) {

            BlogPost post = postService.GetById_ForAdmin( id );
            if (post == null) {
                echoRedirect( lang( "exDataNotFound" ) );
                return;
            }

            sysblogService.SystemDelete( post );
            log( SiteLogString.SystemDeleteBlogPost(), post );

            redirect( Index, 0 );
        }

        [HttpPut, DbTransaction]
        public void UnDelete( int id ) {

            BlogPost post = postService.GetById_ForAdmin( id );
            if (post == null) { echoRedirect( lang( "exDataNotFound" ) ); return; }

            sysblogService.SystemUnDelete( post );
            log( SiteLogString.SystemUnDeleteBlogPost(), post );
            redirect( Trash );
        }



    }

}
