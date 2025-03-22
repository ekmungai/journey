
using Journey.Implrementations;
using CommandLine;

Parser.Default.ParseArguments<Options>(args)
  .WithParsed(RunOptions)
  .WithNotParsed(HandleParseError);
static void RunOptions(Options opts)
{
    //handle options
}
static void HandleParseError(IEnumerable<Error> errs)
{
    //handle errors
}