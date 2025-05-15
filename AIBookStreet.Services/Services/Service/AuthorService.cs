using AIBookStreet.Repositories.Data.Entities;
using AIBookStreet.Repositories.Repositories.Repositories.Interface;
using AIBookStreet.Repositories.Repositories.UnitOfWork.Interface;
using AIBookStreet.Services.Base;
using AIBookStreet.Services.Model;
using AIBookStreet.Services.Services.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AIBookStreet.Services.Services.Service
{
    public class AuthorService(IUnitOfWork repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IFirebaseStorageService firebaseStorageService) : BaseService<Author>(mapper, repository, httpContextAccessor), IAuthorService
    {
        private readonly IUnitOfWork _repository = repository;
        private readonly IFirebaseStorageService _firebaseStorage = firebaseStorageService;
        public async Task<(Author?, string?)> AddAnAuthor(AuthorModel authorModel)
        {            
            try
            {
                authorModel.DOB = authorModel.DOB?.ToLocalTime();
                var fileUrl = "";
                if (authorModel.ImgFile != null)
                {
                    fileUrl = await _firebaseStorage.UploadFileAsync(authorModel.ImgFile);
                }

                if (DateTime.Now.Year - authorModel.DOB?.Year <= 15)
                {
                    return (null, "Tác giả khôn thể nhỏ hơn 15 tuổi");
                }

                var author = new Author
                {
                    BaseImgUrl = !string.IsNullOrEmpty(fileUrl) ? fileUrl : null,
                    AuthorName = authorModel.AuthorName,
                    DOB = authorModel.DOB ?? null,
                    Nationality = authorModel.Nationality,
                    Biography = authorModel.Biography,
                };

                var setAuthor = await SetBaseEntityToCreateFunc(author);
                var isSuccess = await _repository.AuthorRepository.Add(setAuthor);

                if (!isSuccess)
                {
                    if (authorModel.ImgFile != null)
                    {
                        await _firebaseStorage.DeleteFileAsync(fileUrl);
                    }
                    return (null, "Không thể thêm tác giả này");
                }

                return (setAuthor, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(long, Author?)> UpdateAnAuthor(Guid authorID, AuthorModel authorModel)
        {            
            try
            {
                var existAuthor = await _repository.AuthorRepository.GetByID(authorID);
                if (existAuthor == null)
                {
                    return (1, null); //author khong ton tai
                }
                if (existAuthor.IsDeleted)
                {
                    return (3, null);
                }
                var newFileUrl = authorModel.ImgFile != null ? await _firebaseStorage.UploadFileAsync(authorModel.ImgFile) : "";

                var oldFileUrl = existAuthor.BaseImgUrl;
                // Update entity
                existAuthor.AuthorName = authorModel.AuthorName;
                existAuthor.DOB = authorModel.DOB != null ? authorModel.DOB.Value.ToLocalTime() : existAuthor.DOB;
                existAuthor.Nationality = authorModel.Nationality ?? existAuthor.Nationality;
                existAuthor.Biography = authorModel.Biography ?? existAuthor.Biography;
                existAuthor.BaseImgUrl = newFileUrl == "" ? existAuthor.BaseImgUrl : newFileUrl;
                existAuthor = await SetBaseEntityToUpdateFunc(existAuthor);

                var updateSuccess = await _repository.AuthorRepository.Update(existAuthor);
                if (updateSuccess)
                {
                    if (authorModel.ImgFile != null && !string.IsNullOrEmpty(oldFileUrl))
                    {
                        await _firebaseStorage.DeleteFileAsync(oldFileUrl);
                    }
                    return (2, existAuthor); //update thành công
                }

                // Cleanup new file if update fails
                if (authorModel.ImgFile != null)
                {
                    await _firebaseStorage.DeleteFileAsync(newFileUrl);
                }
                return (3, null); //update fail
            }
            catch
            {
                throw;
            }
        }
        public async Task<(long, Author?)> DeleteAnAuthor(Guid id)
        {
            try
            {
                var existAuthor = await _repository.AuthorRepository.GetByID(id);
                if (existAuthor == null)
                {
                    return (1, null); //author khong ton tai
                }
                if (existAuthor.IsDeleted)
                {
                    return (3, null);
                }
                existAuthor = await SetBaseEntityToUpdateFunc(existAuthor);

                var isSuccess = await _repository.AuthorRepository.Delete(existAuthor);
                if (isSuccess)
                {
                    if (!string.IsNullOrEmpty(existAuthor.BaseImgUrl))
                    {
                        await _firebaseStorage.DeleteFileAsync(existAuthor.BaseImgUrl);
                    }
                    return (2, existAuthor);//delete thanh cong
                }
                return (3, null);       //delete fail
            }
            catch
            {
                throw;
            }
        }
        public async Task<Author?> GetAnAuthorById(Guid id)
        {
            try
            {
                return await _repository.AuthorRepository.GetByID(id);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<Author>?> GetAllActiveAuthors(string? authorName, Guid? categoryId)
        {
            try
            {
                var bookAuthors = new List<BookAuthor>();
                var authorIds = new List<Guid>();
                if (categoryId != null)
                {
                    var bookCategories = await _repository.BookCategoryRepository.GetByElement(null, categoryId);


                    if (bookCategories != null)
                    {
                        foreach (var bookCategory in bookCategories)
                        {
                            var bas = await _repository.BookAuthorRepository.GetByElement(bookCategory.BookId, null);
                            if (bas != null)
                            {
                                foreach (var ba in bas)
                                {
                                    bookAuthors.Add(ba);
                                }
                            }
                        }
                    }

                    if (bookAuthors != null)
                    {
                        foreach (var bookAuthor in bookAuthors)
                        {
                            authorIds.Add(bookAuthor.AuthorId);
                        }
                    }
                }
                var authors = await _repository.AuthorRepository.GetAll(authorName, authorIds);

                return authors.Count == 0 ? null : authors;
            }
            catch
            {
                throw;
            }
        }
        public async Task<(List<Author>?, long)> GetAllAuthorsPagination(string? key, Guid? categoryId, int? pageNumber, int? pageSize, string? sortField, bool? desc)
        {
            try
            {
                var user = await GetUserInfo();
                var isAdmin = false;
                if (user != null)
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        if (userRole.Role.RoleName == "Admin")
                        {
                            isAdmin = true;
                        }
                    }
                }
                var bookAuthors = new List<BookAuthor>();
                var authorIds = new List<Guid>();
                if (categoryId != null)
                {
                    var bookCategories = await _repository.BookCategoryRepository.GetByElement(null, categoryId);


                    if (bookCategories != null)
                    {
                        foreach (var bookCategory in bookCategories)
                        {
                            var bas = await _repository.BookAuthorRepository.GetByElement(bookCategory.BookId, null);
                            if (bas != null)
                            {
                                foreach (var ba in bas)
                                {
                                    bookAuthors.Add(ba);
                                }
                            }
                        }
                    }

                    if (bookAuthors != null)
                    {
                        foreach (var bookAuthor in bookAuthors)
                        {
                            authorIds.Add(bookAuthor.AuthorId);
                        }
                    }
                }

                var authors = isAdmin ? await _repository.AuthorRepository.GetAllPaginationForAdmin(key, authorIds, pageNumber, pageSize, sortField, desc)
                                            : await _repository.AuthorRepository.GetAllPagination(key, authorIds, pageNumber, pageSize, sortField, desc);
                return authors.Item1.Count > 0 ? (authors.Item1, authors.Item2) : (null, 0);
            } catch
            {
                throw;
            }
        }
    }
}
