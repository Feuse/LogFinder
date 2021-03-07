# LogFinder

Search In Log.

You may not read through all the records in the file.

########
An issue with Windows.log file:

I believe Windows.log is unsorted.
it starts with 2016 and continues with 2015 in the middle of the file.

the fact that the example log file was sorted and thinking that because it is a log file it should be naturally sorted made me write the algorithm based on a sorted data array.
#######

The algorithm works based on a binary search.
using a filestream to navigate through the file with indexes.

1. find the starting date
2. find the ending date.
3. set a pivot the middle and get the date in the middle.
4. set a searching pivot based on input search start and end average.
5. check if search pivot bigger or smaller than pivot.
6. keep the half that contains the search pivot.
7. check for overflows ( search pivot is based on a range, so it needs to check if the range doesn't overflow the start & end.
8. when end overflows pivot, set position to pivot and start going forward until search input is found.
9. when start overflows ( smaller), set position to start index and go backwards until search input is found.

I tested it with many edge cases such as:

1. Searching for a start date that is lower than actual present dates.
2. Searching for a end date that is higher than the actual present dates.

it should give the closest one.

the method is not the pretiest and needs tons of refactoring, I just did not have enough time to do that.
but these tasks taught me alot and even if I don't make it Im still happy I was able to do it! thank you.
