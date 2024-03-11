﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BookViewerApp.Books
{
	public class BookManager
	{
		public static async Task<IBook> GetBookFromFile(Windows.Storage.IStorageFile file)
		{
			if (file == null) { return null; }
			else if (Path.GetExtension(file.Path).ToLower() == ".pdf")
			{
				var book = new Books.Pdf.PdfBook();
				try
				{
					await book.Load(file);
				}
				catch { return null; }
				if (book.PageCount <= 0) { return null; }
				return book;
			}
			else if (new string[] { ".zip", ".cbz" }.Contains(Path.GetExtension(file.Path).ToLower()))
			{
				var book = new Books.Cbz.CbzBook();
				try
				{
					await book.LoadAsync(WindowsRuntimeStreamExtensions.AsStream(await file.OpenReadAsync()));
				}
				catch
				{
					return null;
				}
				if (book.PageCount <= 0) { return null; }
				return book;
			}
			else if (new string[] { ".rar", ".cbr", ".7z", ".cb7" }.Contains(Path.GetExtension(file.Path).ToLower()))
			{
				var book = new Books.Compressed.CompressedBook();
				try
				{
					await book.LoadAsync(WindowsRuntimeStreamExtensions.AsStream(await file.OpenReadAsync()));
				}
				catch
				{
					return null;
				}
				if (book.PageCount <= 0) { return null; }
				return book;
			}

			return null;
		}

		public static string[] AvailableExtensionsArchive { get { return new string[] { ".pdf", ".zip", ".cbz", ".rar", ".cbr", ".7z", ".cb7" }; } }

		public static string[] AvailableExtensionsImage { get { return new string[] { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff", ".tif", ".hdp", ".wdp", ".jxr", ".avif", ".webp" }; } }


		public static bool IsFileAvailabe(Windows.Storage.IStorageFile file)
		{
			return AvailableExtensionsArchive.Contains(Path.GetExtension(file.Path).ToLower());
		}

		public static string StorageItemRegister(Windows.Storage.IStorageItem file)
		{
			var acl = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
			return acl.Add(file);
		}

		public static async Task<Windows.Storage.IStorageItem> StorageItemGet(string id)
		{
			var acl = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
			return await acl.GetItemAsync(id);
		}

		public static char FileSplitLetter { get { return '\\'; } }

		public static string PathJoin(string[] Path)
		{
			return String.Join(FileSplitLetter.ToString(), Path);
		}

		public static string[] PathSplit(string Path)
		{
			return Path.Split(FileSplitLetter);
		}

		public static async Task<Windows.Storage.IStorageItem> StorageItemGet(string token, string Path)
		{
			return await StorageItemGet(token, PathSplit(Path));
		}

		public static async Task<Windows.Storage.IStorageItem> StorageItemGet(string token, string[] Path)
		{
			Windows.Storage.IStorageItem currentFolder = await StorageItemGet(token);
			foreach (var item in Path)
			{
				if (currentFolder == null) return null;
				if (currentFolder is Windows.Storage.StorageFolder)
				{
					currentFolder = await (currentFolder as Windows.Storage.StorageFolder).TryGetItemAsync(item);
				}
				else
				{
					return null;
				}
			}
			return currentFolder;
		}

		public static async Task<Windows.Storage.StorageFile> PickFile()
		{
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			foreach (var ext in Books.BookManager.AvailableExtensionsArchive)
			{
				picker.FileTypeFilter.Add(ext);
			}
			return await picker.PickSingleFileAsync();

		}

		public static async Task<Books.IBook> PickBook()
		{
			return (await GetBookFromFile(await PickFile()));
		}
	}
}
